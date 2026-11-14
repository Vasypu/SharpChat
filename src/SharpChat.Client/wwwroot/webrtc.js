window.webrtc = {
    peers: {},

    createPeerConnection: function (userId, dotnetHelper) {
        const pc = new RTCPeerConnection({
            iceServers: [
                { urls: "stun:stun.l.google.com:19302" }
            ]
        });

        const dataChannel = pc.createDataChannel("chat");

        dataChannel.onopen = () => {
            dotnetHelper.invokeMethodAsync("OnDataChannelOpen", userId);
        };

        dataChannel.onmessage = (event) => {
            dotnetHelper.invokeMethodAsync("OnMessageReceivedFromJs", userId, event.data);
        };

        pc.onicecandidate = (event) => {
            if (event.candidate) {
                dotnetHelper.invokeMethodAsync("OnIceCandidate", userId,
                    event.candidate.candidate,
                    event.candidate.sdpMid,
                    event.candidate.sdpMLineIndex);
            }
        };

        pc.onconnectionstatechange = () => {
            dotnetHelper.invokeMethodAsync("OnConnectionStateChange", userId, pc.connectionState);
        };

        // Когда нам присылают dataChannel (мы отвечающая сторона)
        pc.ondatachannel = (event) => {
            const channel = event.channel;
            channel.onopen = () => {
                dotnetHelper.invokeMethodAsync("OnDataChannelOpen", userId);
            };
            channel.onmessage = (e) => {
                dotnetHelper.invokeMethodAsync("OnMessageReceivedFromJs", userId, e.data);
            };
            this.peers[userId].dataChannel = channel;
        };

        this.peers[userId] = {
            pc: pc,
            dataChannel: dataChannel,
            dotnetHelper: dotnetHelper
        };
    },

    createOffer: async function (userId) {
        const peer = this.peers[userId];
        const offer = await peer.pc.createOffer();
        await peer.pc.setLocalDescription(offer);
        return offer.sdp;
    },

    createAnswer: async function (userId, remoteSdp) {
        const peer = this.peers[userId];
        await peer.pc.setRemoteDescription({ type: "offer", sdp: remoteSdp });
        const answer = await peer.pc.createAnswer();
        await peer.pc.setLocalDescription(answer);
        return answer.sdp;
    },

    setRemoteDescription: async function (userId, sdp) {
        const peer = this.peers[userId];
        await peer.pc.setRemoteDescription({ type: "answer", sdp: sdp });
    },

    addIceCandidate: async function (userId, candidate, sdpMid, sdpMLineIndex) {
        const peer = this.peers[userId];
        if (peer && candidate) {
            await peer.pc.addIceCandidate({
                candidate: candidate,
                sdpMid: sdpMid,
                sdpMLineIndex: sdpMLineIndex
            });
        }
    },

    sendMessage: function (userId, message) {
        const peer = this.peers[userId];
        if (peer && peer.dataChannel && peer.dataChannel.readyState === "open") {
            peer.dataChannel.send(message);
        }
    },

    closeConnection: function (userId) {
        const peer = this.peers[userId];
        if (peer) {
            if (peer.dataChannel) peer.dataChannel.close();
            peer.pc.close();
            delete this.peers[userId];
        }
    }
};