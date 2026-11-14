using System.Collections.Concurrent;
using System.Text.Json;
using Common;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace Messenger.BlazorClient.Services;

public class PeerConnectionService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly HubConnection _hub;
    private readonly string _currentUserId;

    // userId → информация о соединении
    private readonly ConcurrentDictionary<string, PeerInfo> _peers = new();

    // Очередь сообщений, пока DataChannel ещё не открыт
    private readonly ConcurrentDictionary<string, Queue<ChatMessage>> _pendingMessages = new();

    public event Action<ChatMessage>? OnMessageReceived;
    public event Action<string, string>? OnConnectionStateChanged; // userId, state

    public PeerConnectionService(IJSRuntime js, HubConnection hub, string currentUserId)
    {
        _js = js;
        _hub = hub;
        _currentUserId = currentUserId;

        // Подписываемся на сигналы от хаба
        _hub.On<SignalMessage>(nameof(SignalHubMethods.SendSignal), async signal =>
        {
            await HandleSignalAsync(signal);
        });
    }

    /// <summary>
    /// Инициирует соединение с пользователем (создаёт offer)
    /// </summary>
    public async Task ConnectAsync(string remoteUserId)
    {
        if (_peers.ContainsKey(remoteUserId))
            return;

        var peer = new PeerInfo { UserId = remoteUserId };
        _peers[remoteUserId] = peer;

        // Создаём PeerConnection + DataChannel в JS
        await _js.InvokeVoidAsync("webrtc.createPeerConnection", remoteUserId, DotNetObjectReference.Create(this));

        // Создаём offer
        var offerSdp = await _js.InvokeAsync<string>("webrtc.createOffer", remoteUserId);

        var signal = new SignalMessage(
            Type: SignalType.Offer,
            FromUserId: _currentUserId,
            ToUserId: remoteUserId,
            Sdp: offerSdp
        );

        await _hub.SendAsync("SendSignal", signal);
    }

    /// <summary>
    /// Обработка входящих сигнальных сообщений
    /// </summary>
    public async Task HandleSignalAsync(SignalMessage signal)
    {
        var remoteUserId = signal.FromUserId;

        switch (signal.Type)
        {
            case SignalType.Offer:
                await HandleOfferAsync(remoteUserId, signal.Sdp!);
                break;

            case SignalType.Answer:
                await HandleAnswerAsync(remoteUserId, signal.Sdp!);
                break;

            case SignalType.IceCandidate:
                await HandleIceCandidateAsync(remoteUserId, signal);
                break;
        }
    }

    private async Task HandleOfferAsync(string toUserId, string sdp)
    {
        if (!_peers.ContainsKey(toUserId))
        {
            _peers[toUserId] = new PeerInfo { UserId = toUserId };
            await _js.InvokeVoidAsync("webrtc.createPeerConnection", toUserId, DotNetObjectReference.Create(this));
        }

        // Устанавливаем remote description и создаём answer
        var answerSdp = await _js.InvokeAsync<string>("webrtc.createAnswer", toUserId, sdp);

        var signal = new SignalMessage(
            Type: SignalType.Answer,
            FromUserId: _currentUserId,
            ToUserId: toUserId,
            Sdp: answerSdp
        );

        await _hub.SendAsync(nameof(SignalHubMethods.SendSignal), signal);
    }

    private async Task HandleAnswerAsync(string remoteUserId, string sdp)
    {
        await _js.InvokeVoidAsync("webrtc.setRemoteDescription", remoteUserId, sdp);
    }

    private async Task HandleIceCandidateAsync(string remoteUserId, SignalMessage signal)
    {
        await _js.InvokeVoidAsync("webrtc.addIceCandidate", remoteUserId, signal.Candidate, signal.SdpMid, signal.SdpMLineIndex);
    }

    /// <summary>
    /// Отправка сообщения чата
    /// </summary>
    public async Task SendMessageAsync(string toUserId, string content)
    {
        // пересмотреть эту лажу
        var message = new ChatMessage
        (
            //FromUserId = _currentUserId,
            toUserId,
            content,
            timestamp: DateTimeOffset.Now.ToString(),
            true,
            ChatStatus.Pending
        );

        // Если канал ещё не открыт — кладём в очередь и инициируем соединение
        if (!_peers.TryGetValue(toUserId, out var peer) || peer.DataChannelOpen == false)
        {
            var queue = _pendingMessages.GetOrAdd(toUserId, _ => new Queue<ChatMessage>());
            queue.Enqueue(message);

            await ConnectAsync(toUserId);
            return;
        }

        await SendThroughDataChannelAsync(toUserId, message);
    }

    private async Task SendThroughDataChannelAsync(string toUserId, ChatMessage message)
    {
        var json = JsonSerializer.Serialize(message);
        await _js.InvokeVoidAsync("webrtc.sendMessage", toUserId, json);

        message.Status = ChatStatus.Sent;
        // Здесь можно сохранить в IndexedDB / обновить UI
    }

    /// <summary>
    /// Вызывается из JavaScript, когда приходит ICE-кандидат
    /// </summary>
    [JSInvokable]
    public async Task OnIceCandidate(string remoteUserId, string candidate, string? sdpMid, int? sdpMLineIndex)
    {
        var signal = new SignalMessage(
            Type: SignalType.IceCandidate,
            FromUserId: _currentUserId,
            ToUserId: remoteUserId,
            Candidate: candidate,
            SdpMid: sdpMid,
            SdpMLineIndex: sdpMLineIndex
        );

        await _hub.SendAsync("SendSignal", signal);
    }

    /// <summary>
    /// Вызывается из JavaScript, когда DataChannel открылся
    /// </summary>
    [JSInvokable]
    public async Task OnDataChannelOpen(string remoteUserId)
    {
        if (_peers.TryGetValue(remoteUserId, out var peer))
        {
            peer.DataChannelOpen = true;
        }

        OnConnectionStateChanged?.Invoke(remoteUserId, "open");

        // Сбрасываем очередь сообщений
        if (_pendingMessages.TryGetValue(remoteUserId, out var queue))
        {
            while (queue.TryDequeue(out var message))
            {
                await SendThroughDataChannelAsync(remoteUserId, message);
            }
        }
    }

    /// <summary>
    /// Вызывается из JavaScript при получении сообщения
    /// </summary>
    [JSInvokable]
    public void OnMessageReceivedFromJs(string remoteUserId, string json)
    {
        var message = JsonSerializer.Deserialize<ChatMessage>(json);
        if (message is null) return;

        message.Status = ChatStatus.Delivered;
        OnMessageReceived?.Invoke(message);

        // Здесь сохраняем в IndexedDB
    }

    /// <summary>
    /// Вызывается из JavaScript при изменении состояния соединения
    /// </summary>
    [JSInvokable]
    public void OnConnectionStateChange(string remoteUserId, string state)
    {
        OnConnectionStateChanged?.Invoke(remoteUserId, state);

        if (state is "failed" or "closed" or "disconnected")
        {
            _peers.TryRemove(remoteUserId, out _);
        }
    }

    public async Task DisconnectAsync(string remoteUserId)
    {
        await _js.InvokeVoidAsync("webrtc.closeConnection", remoteUserId);
        _peers.TryRemove(remoteUserId, out _);
        _pendingMessages.TryRemove(remoteUserId, out _);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var userId in _peers.Keys)
        {
            await DisconnectAsync(userId);
        }
    }

    private class PeerInfo
    {
        public string UserId { get; set; } = string.Empty;
        public bool DataChannelOpen { get; set; }
    }
}