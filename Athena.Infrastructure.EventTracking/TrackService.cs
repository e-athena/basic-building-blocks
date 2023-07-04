namespace Athena.Infrastructure.EventTracking;

/// <summary>
/// 
/// </summary>
public class TrackService : ITrackService
{
    private readonly ConcurrentQueue<Track> _trackQueue;
    private readonly ManualResetEvent _trackManual;
    private readonly ITrackStorageService _trackStorageService;

    /// <summary>
    /// 
    /// </summary>
    public TrackService(ITrackStorageService trackStorageService)
    {
        _trackStorageService = trackStorageService;
        _trackQueue = new ConcurrentQueue<Track>();
        _trackManual = new ManualResetEvent(false);
        TrackRegister();
    }

    /// <summary>
    /// 写入追踪数据
    /// </summary>
    /// <param name="track"></param>
    /// <returns></returns>
    public void Write(Track track)
    {
        // 入队
        _trackQueue.Enqueue(track);
        _trackManual.Set();
    }

    /// <summary>
    /// 写入追踪数据
    /// </summary>
    /// <param name="track"></param>
    /// <returns></returns>
    public Task WriteAsync(Track track)
    {
        Write(track);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 另一个线程记录追踪数据，只在程序初始化时调用一次
    /// </summary>
    private void TrackRegister()
    {
        var thread = new Thread(() =>
        {
            while (true)
            {
                // 等待信号通知
                _trackManual.WaitOne();
                // 出队
                while (!_trackQueue.IsEmpty && _trackQueue.TryDequeue(out var track))
                {
                    // 从队列中写入数据到数据库
                    _trackStorageService.WriteAsync(track);
                }

                // 重新设置信号
                _trackManual.Reset();
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