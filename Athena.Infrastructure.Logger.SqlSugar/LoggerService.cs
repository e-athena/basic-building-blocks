namespace Athena.Infrastructure.Logger.SqlSugar;

/// <summary>
/// 日志服务类
/// </summary>
public class LoggerService : ILoggerService
{
    private readonly ConcurrentQueue<Log> _normalLogQueue;
    private readonly ManualResetEvent _normalLogManual;
    private readonly ILoggerStorageService _loggerStorageService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="loggerStorageService"></param>
    public LoggerService(ILoggerStorageService loggerStorageService)
    {
        _normalLogQueue = new ConcurrentQueue<Log>();
        _normalLogManual = new ManualResetEvent(false);
        _loggerStorageService = loggerStorageService;
        NormalLogRegister();
    }

    /// <summary>
    /// 写日志
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public void Write(Log log)
    {
        // 入队
        _normalLogQueue.Enqueue(log);
        _normalLogManual.Set();
    }

    /// <summary>
    /// 写日志
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public Task WriteAsync(Log log)
    {
        Write(log);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 另一个线程记录日志，只在程序初始化时调用一次
    /// </summary>
    private void NormalLogRegister()
    {
        var thread = new Thread(() =>
        {
            while (true)
            {
                // 等待信号通知
                _normalLogManual.WaitOne();
                // 出队
                while (!_normalLogQueue.IsEmpty && _normalLogQueue.TryDequeue(out var log))
                {
                    // 从队列中写日志至数据库
                    _loggerStorageService.WriteAsync(log);
                }

                // 重新设置信号
                _normalLogManual.Reset();
                Thread.Sleep(1);
            }
            // ReSharper disable once FunctionNeverReturns
        })
        {
            IsBackground = false
        };
        thread.Start();
    }
}