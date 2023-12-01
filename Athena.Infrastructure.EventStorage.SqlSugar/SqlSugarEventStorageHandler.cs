using Athena.Infrastructure.Event.Attributes;
using Athena.Infrastructure.Event.Interfaces;
using Athena.Infrastructure.EventStorage.Events;
using Athena.Infrastructure.EventStorage.Models;
using Athena.Infrastructure.Providers;
using DotNetCore.CAP.Internal;

namespace Athena.Infrastructure.EventStorage.SqlSugar;

/// <summary>
/// 事件存储处理器
/// </summary>
public class SqlSugarEventStorageHandler :
    IMessageHandler<EventPublished>
{
    private readonly ILogger<SqlSugarEventStorageHandler> _logger;
    private readonly ISqlSugarEventStorageClient _sqlSugarClient;
    private readonly ISnowflakeId? _snowflakeId;

    public SqlSugarEventStorageHandler(ISqlSugarEventStorageClient sqlSugarClient, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SqlSugarEventStorageHandler>();
        _sqlSugarClient = sqlSugarClient;
        _snowflakeId = AthenaProvider.GetService<ISnowflakeId>();
    }

    /// <summary>
    /// 处理事件发布
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [DistributedLock]
    [IntegratedEventSubscribe(nameof(EventPublished), nameof(SqlSugarEventStorageHandler))]
    public async Task HandleAsync(EventPublished payload, CancellationToken cancellationToken)
    {
        // 根据事件ID判断是否已经存在
        var eventStream = payload.EventStream;
        var exist = await _sqlSugarClient.Queryable<EventStream>()
            .Where(p => p.EventId == eventStream.EventId)
            .AnyAsync();
        if (exist)
        {
            _logger.LogWarning("事件ID为{EventId}的事件已经存在，不再保存", eventStream.EventId);
            return;
        }

        if (eventStream.Sequence == 0)
        {
            eventStream.Sequence = _snowflakeId?.NextId() ?? DateTime.Now.Ticks;
        }

        // 保存事件流
        await _sqlSugarClient.Insertable(eventStream).ExecuteCommandAsync(cancellationToken);
    }
}