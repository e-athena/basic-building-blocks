using Athena.Infrastructure.EventStorage.Messaging.Requests;
using Athena.Infrastructure.EventStorage.Messaging.Responses;
using Athena.Infrastructure.EventStorage.Models;

namespace Athena.Infrastructure.EventStorage.FreeSql;

/// <summary>
/// 事件流查询服务接口实现类
/// </summary>
public class FreeSqlEventStreamQueryService : IEventStreamQueryService
{
    private readonly ILogger<FreeSqlEventStreamQueryService> _logger;
    private readonly IFreeSql<IEventStorageFreeSql> _freeSql;

    /// <summary>
    ///
    /// </summary>
    /// <param name="freeSql"></param>
    /// <param name="loggerFactory"></param>
    public FreeSqlEventStreamQueryService(IFreeSql<IEventStorageFreeSql> freeSql, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<FreeSqlEventStreamQueryService>();
        _freeSql = freeSql;
    }

    /// <summary>
    /// 读取分页数据
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<Paging<GetEventStreamPagingResponse>> GetPagingAsync(GetEventStreamPagingRequest request)
    {
        return _freeSql.Queryable<EventStream>()
            .Where(p => p.AggregateRootId == request.Id || p.UserId == request.Id)
            .ToPagingAsync(request, p => new GetEventStreamPagingResponse
            {
                Sequence = p.Sequence,
                AggregateRootTypeName = p.AggregateRootTypeName,
                AggregateRootId = p.AggregateRootId,
                Version = p.Version,
                EventId = p.EventId,
                EventName = p.EventName,
                CreatedOn = p.CreatedOn,
                UserId = p.UserId
            });
    }

    /// <summary>
    /// 读取内容
    /// </summary>
    /// <param name="sequence"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<string> GetEventPayloadAsync(long sequence)
    {
        return _freeSql.Queryable<EventStream>()
            .Where(p => p.Sequence == sequence)
            .FirstAsync(p => p.Events);
    }
}