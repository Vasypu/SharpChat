using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace Messenger.BlazorClient.Services;

public class P2PChatService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly IPeerConnectionManagerFactory _factory;

    private HubConnection? Hub { get; set; }
    public PeerConnectionService? Peers { get; private set; }

    public P2PChatService(IJSRuntime js, IPeerConnectionManagerFactory factory)
    {
        _js = js;
        _factory = factory;
    }

    public async Task InitializeAsync(string userId)
    {
        Hub = new HubConnectionBuilder()
            .WithUrl($"/signalinghub?userId={userId}")
            .WithAutomaticReconnect()
            .Build();

        await Hub.StartAsync();

        Peers = _factory.Create(Hub, userId);
    }

    public async ValueTask DisposeAsync()
    {
        if (Peers is not null)
            await Peers.DisposeAsync();

        if (Hub is not null)
            await Hub.DisposeAsync();
    }
}

public interface IPeerConnectionManagerFactory
{
    PeerConnectionService Create(HubConnection hubConnection, string currentUserId);
}

public class PeerConnectionManagerFactory : IPeerConnectionManagerFactory
{
    private readonly IJSRuntime _js;

    public PeerConnectionManagerFactory(IJSRuntime js)
    {
        _js = js;
    }

    public PeerConnectionService Create(HubConnection hubConnection, string currentUserId)
    {
        return new PeerConnectionService(_js, hubConnection, currentUserId);
    }
}