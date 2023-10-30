namespace Athena.Infrastructure.Wechat.Exceptions;

/// <summary>
/// 当前请求IP不在白名单中
/// </summary>
public class CurrentRequestIpNotInWhitelistException : Exception
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
    public CurrentRequestIpNotInWhitelistException(int errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}