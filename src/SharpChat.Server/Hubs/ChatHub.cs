using Common;
using Microsoft.AspNetCore.SignalR;

namespace MessengerServer.Hubs;

public class ChatHub : Hub
{
    // Этот метод будут вызывать клиенты, чтобы отправить сообщение
    /*public async Task SendMessage(string user, string message)
    {
        // Пересылаем сообщение ВСЕМ подключенным пользователям
        await Clients.All.SendAsync("ReceiveMessage", new ChatMessage(user, message, DateTime.UtcNow));
    }*/

    // Метод для приватной отправки
    public async Task SendPrivateMessage(string targetUser, string message)
    {
        // Узнаем, кто отправляет сообщение (SignalR сам вызовет наш CustomUserIdProvider)
        string senderUser = Context.UserIdentifier ?? "Аноним";
        string time = DateTime.Now.ToShortTimeString();

        Console.WriteLine($"[Private] {senderUser} -> {targetUser}: {message}");

        // 1. Отправляем сообщение получателю (targetUser)
        await Clients.User(targetUser).SendAsync("ReceivePrivateMessage", new ChatMessage(senderUser, message, time, isOwnMessage: false, ChatStatus.Sent));

        // 2. Отправляем копию самому себе (senderUser), чтобы у нас в консоли/чате оно тоже отобразилось
        await Clients.User(senderUser).SendAsync("ReceivePrivateMessage", new ChatMessage(senderUser, message, time, isOwnMessage: true, ChatStatus.Sent));
    }
}