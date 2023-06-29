namespace Athena.Infrastructure.Logger;

/// <summary>
/// 日志服务接口
/// </summary>
public interface ILoggerService
{
    /// <summary>
    /// 写日志
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    void Write(Log log);

    /// <summary>
    /// 写日志
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    Task WriteAsync(Log log);
}