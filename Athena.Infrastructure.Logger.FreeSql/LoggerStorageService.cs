using Athena.Infrastructure.Messaging.Responses;
using Athena.Infrastructure.ViewModels;

namespace Athena.Infrastructure.Logger.FreeSql;

/// <summary>
/// 日志存储服务接口实现
/// </summary>
public class LoggerStorageService : ILoggerStorageService
{
    private readonly IFreeSql<ILoggerFreeSql> _freeSql;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="freeSql"></param>
    public LoggerStorageService(IFreeSql<ILoggerFreeSql> freeSql)
    {
        _freeSql = freeSql;
    }

    /// <summary>
    /// 缓存
    /// </summary>
    private static readonly Dictionary<string, IBaseRepository<Log, long>> LogBaseRepositoryDict = new();

    private static readonly List<string> ServiceNameList = new();

    /// <summary>
    /// 添加日志
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public async Task WriteAsync(Log log)
    {
        var tableName = $"{IndexHelper.GetTableName(typeof(Log))}_{log.ServiceName}";
        // 判断服务名是否存在,先读取缓存
        if (!ServiceNameList.Contains(log.ServiceName))
        {
            if (!await _freeSql.Queryable<ServiceInfo>().AnyAsync(c => c.Name == log.ServiceName))
            {
                await _freeSql.Insert(new ServiceInfo
                {
                    Name = log.ServiceName
                }).ExecuteAffrowsAsync();
                ServiceNameList.Add(log.ServiceName);
            }
        }

        if (!LogBaseRepositoryDict.TryGetValue(log.ServiceName, out var resp))
        {
            // 创建索引
            IndexHelper.Create(_freeSql, new Dictionary<Type, string?>
            {
                {typeof(Log), tableName}
            });
            resp = _freeSql.GetRepository<Log, long>();
            resp.AsTable(_ => tableName);
            LogBaseRepositoryDict.TryAdd(log.ServiceName, resp);
        }

        await resp.InsertAsync(log);
    }

    /// <summary>
    /// 读取分页数据
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<Paging<GetLogPagingResponse>> GetPagingAsync(GetLogPagingRequest request)
    {
        return Query(request.ServiceName)
            .HasWhere(request.DateRange,
                p => p.CreatedOn > request.DateRange![0] && p.CreatedOn < request.DateRange[1].AddDays(1))
            .HasWhere(request.UserId, p => p.UserId == request.UserId)
            .HasWhere(request.TraceId, p => p.TraceId == request.TraceId)
            .HasWhere(request.LogLevel, p => p.LogLevel == request.LogLevel)
            .ToPagingAsync<Log, GetLogPagingResponse>(request);
    }

    /// <summary>
    /// 读取详情
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<GetByIdResponse> GetByIdAsync(GetByIdRequest request)
    {
        return Query(request.ServiceName)
            .Where(p => p.Id == request.Id)
            .FirstAsync<GetByIdResponse>();
    }

    /// <summary>
    /// 读取详情
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<GetByTraceIdResponse> GetByTraceIdAsync(GetByTraceIdRequest request)
    {
        return Query(request.ServiceName)
            .Where(p => p.TraceId == request.TraceId)
            .FirstAsync<GetByTraceIdResponse>();
    }

    /// <summary>
    /// 读取调用次数
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<long> GetCallCountAsync(GetCallCountRequest request)
    {
        return Query(request.ServiceName)
            .WhereIf(!string.IsNullOrEmpty(request.Route), p => p.Route == request.Route)
            .WhereIf(!string.IsNullOrEmpty(request.UserId), p => p.UserId == request.UserId)
            .WhereIf(!string.IsNullOrEmpty(request.AliasName), p => p.AliasName!.Contains(request.AliasName!))
            .WhereIf(request.StatusCode != null, p => p.StatusCode == request.StatusCode)
            .CountAsync();
    }

    /// <summary>
    /// 读取服务列表
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<List<SelectViewModel>> GetServiceSelectListAsync()
    {
        return _freeSql
            .Select<ServiceInfo>()
            .ToListAsync(p => new SelectViewModel
            {
                Label = p.Name,
                Value = p.Name
            });
    }

    #region Private Methods

    /// <summary>
    /// Repository
    /// <remarks>按服务名分表，e.g.:Logs_CommonService</remarks>
    /// </summary>
    private ISelect<Log> Query(string serviceName)
    {
        return _freeSql.Queryable<Log>().AsTable((_, oldName) => $"{oldName}_{serviceName}");
    }

    #endregion
}