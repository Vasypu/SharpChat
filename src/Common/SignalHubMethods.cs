namespace Common;

public enum SignalHubMethods
{
    /// <summary>
    /// 
    /// </summary>
    SendSignal,

    /// <summary>
    /// Ответ на предложение (SDP answer)
    /// </summary>
    Answer,

    /// <summary>
    /// Кандидат ICE для установки соединения
    /// </summary>
    IceCandidate
}