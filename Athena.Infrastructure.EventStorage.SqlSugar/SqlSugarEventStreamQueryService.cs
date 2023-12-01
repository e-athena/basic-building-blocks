using Athena.Infrastructure.EventStorage.Messaging.Requests;
using Athena.Infrastructure.EventStorage.Messaging.Responses;
using Athena.Infrastructure.EventStorage.Models;
using Athena.Infrastructure.Messaging.Responses;
using Athena.Infrastructure.SqlSugar;

namespace Athena.Infrastructure.EventStorage.SqlSugar;

/// <summary>
/// 事件流查询服务接口实现类
/// </summary>
public class SqlSugarEventStreamQueryService : IEventStreamQueryService
{
    private readonly ILogger<SqlSugarEventStreamQueryService> _logger;
    private readonly ISqlSugarEventStorageClient _sqlSugarClient;

    public SqlSugarEventStreamQueryService(ISqlSugarEventStorageClient sqlSugarClient, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SqlSugarEventStreamQueryService>();
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 读取分页数据
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<Paging<GetEventStreamPagingResponse>> GetPagingAsync(GetEventStreamPagingRequest request)
    {
        return _sqlSugarClient.Queryable<EventStream>()
            .Where(p => p.AggregateRootId == request.AggregateRootId)
            .ToPagingAsync(request, p => new GetEventStreamPagingResponse());
    }
}