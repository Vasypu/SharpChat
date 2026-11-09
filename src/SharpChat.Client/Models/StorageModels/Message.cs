using System.Text.Json.Serialization;
using Tavenem.DataStorage;

namespace Messenger.BlazorClient.Models.StorageModels;

/// <summary>
/// Модель сообщения для хранения в IndexedDB
/// </summary>
public class Message : IIdItem
{
    /// <summary>
    /// Уникальный идентификатор сообщения
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// ID чата, к которому относится сообщение
    /// </summary>
    public string ChatId { get; set; } = string.Empty;
    
    /// <summary>
    /// ID отправителя
    /// </summary>
    public string SenderId { get; set; } = string.Empty;
    
    /// <summary>
    /// Текст сообщения
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Временная метка в UTC (в тиках для сортировки)
    /// </summary>
    public long TimestampUtcTicks { get; set; }
    
    /// <summary>
    /// Временная метка в UTC (DateTime для удобства)
    /// </summary>
    public DateTime TimestampUtc
    {
        get => new DateTime(TimestampUtcTicks, DateTimeKind.Utc);
        set => TimestampUtcTicks = value.Ticks;
    }
    
    /// <summary>
    /// Флаг: отправлено ли сообщение на сервер через SignalR
    /// </summary>
    public bool IsSynced { get; set; }
    
    /// <summary>
    /// Флаг: прочитано ли сообщение
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Реализация IEquatable для IIdItem
    /// </summary>
    public bool Equals(IIdItem? other) => other is Message message && Id == message.Id;

    public override bool Equals(object? obj) => Equals(obj as IIdItem);

    public override int GetHashCode() => Id?.GetHashCode() ?? 0;
}
