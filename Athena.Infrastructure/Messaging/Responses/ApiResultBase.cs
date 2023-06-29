namespace Athena.Infrastructure.Messaging.Responses;

/// <summary>
/// 
/// </summary>
[Serializable]
public class ApiResultBase
{
    /// <summary>
    /// Exception Inner Message
    /// </summary>
    public string? InnerMessage { get; set; }

    /// <summary>
    /// StackTrace
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Message
    /// </summary>
    public string Message { get; set; } = "Ok";

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 状态码
    /// </summary>
    public int StatusCode { get; set; } = 200;
}