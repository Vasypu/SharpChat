using System.Text.Json.Serialization;

namespace Common;

/// <summary>
/// Собщение
/// </summary>
/// <param name="toUserId"></param>
/// <param name="content"></param>
/// <param name="timestamp"></param>
/// <param name="isOwnMessage"></param>
/// <param name="status"></param>
public class ChatMessage(
    string toUserId,
    string content,
    string timestamp,
    bool isOwnMessage,
    ChatStatus status
)
{
    /// <summary></summary>
    public string ToUserId { get; init; } = toUserId;

    /// <summary></summary>
    public string Content { get; init; } = content;

    /// <summary></summary>
    public string Timestamp { get; init; } = timestamp;

    /// <summary></summary>
    public bool IsOwnMessage { get; init; } = isOwnMessage;

    /// <summary></summary>
    public ChatStatus Status { get; set; } = status;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatStatus
{
    /// <summary>
    /// 
    /// </summary>
    Pending,

    /// <summary>
    /// 
    /// </summary>
    Sent,

    /// <summary>
    /// 
    /// </summary>
    Delivered
}