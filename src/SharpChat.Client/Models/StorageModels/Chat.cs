using Tavenem.DataStorage;

namespace Messenger.BlazorClient.Models.StorageModels;

/// <summary>
/// Модель чата/диалога для хранения в IndexedDB
/// </summary>
public class Chat : IIdItem
{
    /// <summary>
    /// Уникальный идентификатор чата (обычно username собеседника)
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Имя чата (имя собеседника)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Текст последнего сообщения для превью
    /// </summary>
    public string LastMessageText { get; set; } = string.Empty;
    
    /// <summary>
    /// Временная метка последнего сообщения (в тиках для сортировки)
    /// </summary>
    public long LastMessageTimestampTicks { get; set; }
    
    /// <summary>
    /// Временная метка последнего сообщения (DateTime для удобства)
    /// </summary>
    public DateTime LastMessageTimestamp
    {
        get => new DateTime(LastMessageTimestampTicks, DateTimeKind.Utc);
        set => LastMessageTimestampTicks = value.Ticks;
    }
    
    /// <summary>
    /// Количество непрочитанных сообщений
    /// </summary>
    public int UnreadCount { get; set; }

    /// <summary>
    /// Реализация IEquatable для IIdItem
    /// </summary>
    public bool Equals(IIdItem? other) => other is Chat chat && Id == chat.Id;

    public override bool Equals(object? obj) => Equals(obj as IIdItem);

    public override int GetHashCode() => Id?.GetHashCode() ?? 0;
}
