namespace Athena.Infrastructure.Wechat.Exceptions;

/// <summary>
/// 读取微信AccountToken异常
/// </summary>
public class ReadWeChatAccountTokenException : Exception
{
    /// <summary>
    /// 错误代码
    /// </summary>
    public int ErrorCode { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="message"></param>
    public ReadWeChatAccountTokenException(int errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public ReadWeChatAccountTokenException(int errorCode, string message, Exception innerException) : base(
        message,
        innerException
    )
    {
        ErrorCode = errorCode;
    }
}