using Microsoft.AspNetCore.SignalR;

namespace MessengerServer;

public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        // Берем имя пользователя из контекста запроса.
        // Когда клиент подключается, он передает свой ник в Query String (например, ?username=Алиса)
        var httpContext = connection.GetHttpContext();
        return httpContext?.Request.Query["username"];
    }
}