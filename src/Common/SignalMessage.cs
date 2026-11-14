using System.Text.Json.Serialization;

namespace Common;

/// <summary>
/// Сигнальные сообщения
/// </summary>
/// <param name="Type"></param>
/// <param name="FromUserId"></param>
/// <param name="ToUserId"></param>
/// <param name="Sdp"></param>
/// <param name="Candidate"></param>
/// <param name="SdpMid"></param>
/// <param name="SdpMLineIndex"></param>
public record SignalMessage(
    SignalType Type,
    string FromUserId,
    string ToUserId,
    string? Sdp = null,
    string? Candidate = null,
    string? SdpMid = null,
    int? SdpMLineIndex = null
);

/// <summary>
/// Типы сигнальных сообщений для WebRTC соединения
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SignalType
{
    /// <summary>
    /// Предложение соединения (SDP offer)
    /// </summary>
    Offer,

    /// <summary>
    /// Ответ на предложение (SDP answer)
    /// </summary>
    Answer,

    /// <summary>
    /// Кандидат ICE для установки соединения
    /// </summary>
    IceCandidate
}