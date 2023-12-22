namespace Athena.Infrastructure.EventStorage.FreeSql;

/// <summary>
/// 事件存储处理器
/// </summary>
public class FreeSqlEventStorageHandler :
    IMessageHandler<EventPublished>
{
    private readonly ILogger<FreeSqlEventStorageHandler> _logger;
    private readonly IFreeSql<IEventStorageFreeSql> _freeSql;

    /// <summary>
    ///
    /// </summary>
    /// <param name="freeSql"></param>
    /// <param name="loggerFactory"></param>
    public FreeSqlEventStorageHandler(IFreeSql<IEventStorageFreeSql> freeSql, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<FreeSqlEventStorageHandler>();
        _freeSql = freeSql;
    }

    /// <summary>
    /// 处理事件发布
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [DistributedLock]
    [IntegratedEventSubscribe(nameof(EventPublished), nameof(FreeSqlEventStorageHandler))]
    public async Task HandleAsync(EventPublished payload, CancellationToken cancellationToken)
    {
        // 根据事件ID判断是否已经存在
        var eventStream = payload.EventStream;
        var exist = await _freeSql.Select<EventStream>()
            .Where(p => p.EventId == eventStream.EventId)
            .AnyAsync(cancellationToken);
        if (exist)
        {
            _logger.LogWarning("事件ID为{EventId}的事件已经存在，不再保存", eventStream.EventId);
            return;
        }

        // 保存事件流
        await _freeSql.Insert(eventStream).ExecuteAffrowsAsync(cancellationToken);
    }
}