using System.Collections.Concurrent;
using Common;
using Microsoft.AspNetCore.SignalR;

namespace MessengerServer.Hubs;

public class SignalingHub : Hub
{
    // UserId → ConnectionId
    private static readonly ConcurrentDictionary<string, string> _connections = new();

    // ConnectionId → UserId (для быстрого поиска при отключении)
    private static readonly ConcurrentDictionary<string, string> _connectionToUser = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();

        if (string.IsNullOrWhiteSpace(userId))
        {
            Context.Abort();
            return;
        }

        // Если пользователь уже был подключён — удаляем старое соединение
        if (_connections.TryGetValue(userId, out var oldConnectionId))
        {
            _connectionToUser.TryRemove(oldConnectionId, out _);
        }

        _connections[userId] = Context.ConnectionId;
        _connectionToUser[Context.ConnectionId] = userId;

        // Уведомляем остальных, что пользователь появился в сети
        await Clients.Others.SendAsync("UserOnline", userId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectionToUser.TryRemove(Context.ConnectionId, out var userId))
        {
            _connections.TryRemove(userId, out _);
            await Clients.Others.SendAsync("UserOffline", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Универсальный метод для отправки сигнальных сообщений (offer / answer / ice)
    /// </summary>
    public async Task SendSignal(SignalMessage signal)
    {
        if (string.IsNullOrWhiteSpace(signal.ToUserId))
            return;

        if (_connections.TryGetValue(signal.ToUserId, out var targetConnectionId))
        {
            // Подставляем отправителя на всякий случай
            signal = signal with { FromUserId = GetCurrentUserId() };

            await Clients.Client(targetConnectionId)
                .SendAsync(nameof(SignalHubMethods.SendSignal), signal);
        }
    }

    // Опционально: уведомление, что пользователь онлайн/офлайн
    public async Task NotifyPresence(string userId, bool isOnline) { }

    /// <summary>
    /// Можно вызывать, когда пользователь хочет узнать, кто сейчас онлайн
    /// </summary>
    public Task<IEnumerable<string>> GetOnlineUsers()
    {
        return Task.FromResult(_connections.Keys.AsEnumerable());
    }

    private string GetCurrentUserId()
    {
        _connectionToUser.TryGetValue(Context.ConnectionId, out var userId);
        return userId ?? string.Empty;
    }
}