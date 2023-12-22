namespace Athena.Infrastructure.Logger;

/// <summary>
/// 日志服务默认实现
/// </summary>
public class DefaultLoggerService : ILoggerService
{
    private readonly ILogger? _logger;

    /// <summary>
    /// 日志服务默认实现
    /// </summary>
    /// <param name="logger">日志</param>
    public DefaultLoggerService(ILogger? logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 日志服务默认实现
    /// </summary>
    /// <param name="loggerFactory">日志工厂类</param>
    public DefaultLoggerService(ILoggerFactory? loggerFactory)
    {
        if (loggerFactory != null)
        {
            _logger = loggerFactory.CreateLogger<DefaultLoggerService>();
        }
    }

    /// <summary>
    /// 写日志
    /// </summary>
    /// <param name="log"></param>
    public void Write(Log log)
    {
        if (_logger == null)
        {
            return;
        }

        var message = $"TraceId:{log.TraceId},路由:{log.Route},请求方式:{log.HttpMethod}。";

        switch (log.LogLevel)
        {
            case LogLevel.Critical:
                _logger.LogCritical("{Msg}{@Log}", message, log);
                break;
            case LogLevel.Debug:
                _logger.LogDebug("{Msg}{@Log}", message, log);
                break;
            case LogLevel.Information:
                _logger.LogInformation("{Msg}{@Log}", message, log);
                break;
            case LogLevel.Error:
                _logger.LogError("{Msg}{@Log}", message, log);
                break;
            case LogLevel.Warning:
                _logger.LogWarning("{Msg}{@Log}", message, log);
                break;
            case LogLevel.Trace:
                _logger.LogTrace("{Msg}{@Log}", message, log);
                break;
            default:
                _logger.LogInformation("{Msg}{@Log}", message, log);
                break;
        }
    }

    /// <summary>
    /// 写日志
    /// </summary>
    /// <param name="log"></param>
    public Task WriteAsync(Log log)
    {
        Write(log);
        return Task.CompletedTask;
    }
}