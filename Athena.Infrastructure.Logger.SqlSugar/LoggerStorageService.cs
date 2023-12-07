using Athena.Infrastructure.Helpers;
using Athena.Infrastructure.Messaging.Responses;
using Athena.Infrastructure.SqlSugar;
using Athena.Infrastructure.ViewModels;

namespace Athena.Infrastructure.Logger.SqlSugar;

/// <summary>
/// 日志存储服务接口实现
/// </summary>
public class LoggerStorageService : ILoggerStorageService
{
    private readonly ISqlSugarLoggerClient _sqlSugarClient;

    public LoggerStorageService(ISqlSugarLoggerClient sqlSugarLoggerClient)
    {
        _sqlSugarClient = sqlSugarLoggerClient;
    }

    private static readonly List<string> ServiceNameList = new();

    /// <summary>
    /// 添加日志
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public async Task WriteAsync(Log log)
    {
        // 判断服务名是否存在,先读取缓存
        if (!ServiceNameList.Contains(log.ServiceName))
        {
            if (!await _sqlSugarClient.Queryable<ServiceInfo>().AnyAsync(c => c.Name == log.ServiceName))
            {
                await _sqlSugarClient.Insertable(new ServiceInfo
                {
                    Name = log.ServiceName
                }).ExecuteCommandAsync();
                ServiceNameList.Add(log.ServiceName);
            }
        }

        var tableName = $"Logs_{log.ServiceName}";
        _sqlSugarClient.CodeFirst.As<Log>(tableName).InitTables(typeof(Log));
        await _sqlSugarClient
            .Insertable(log)
            .AS(tableName)
            .ExecuteCommandAsync();
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
            .Where(p => p.CreatedOn > request.DateRange[0] && p.CreatedOn < request.DateRange[1].AddDays(1))
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
            .Select<GetByIdResponse>()
            .FirstAsync();
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
            .Select<GetByTraceIdResponse>()
            .FirstAsync();
    }

    /// <summary>
    /// 读取调用次数
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<long> GetCallCountAsync(GetCallCountRequest request)
    {
        var count = await Query(request.ServiceName)
            .WhereIF(!string.IsNullOrEmpty(request.Route), p => p.Route == request.Route)
            .WhereIF(!string.IsNullOrEmpty(request.UserId), p => p.UserId == request.UserId)
            .WhereIF(!string.IsNullOrEmpty(request.AliasName), p => p.AliasName!.Contains(request.AliasName!))
            .WhereIF(request.StatusCode != null, p => p.StatusCode == request.StatusCode)
            .CountAsync();
        return count;
    }

    /// <summary>
    /// 读取服务列表
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<List<SelectViewModel>> GetServiceSelectListAsync()
    {
        return _sqlSugarClient
            .Queryable<ServiceInfo>()
            .ToListAsync(p => new SelectViewModel
            {
                Label = p.Name,
                Value = p.Name
            });
    }

    #region Private Methods

    /// <summary>
    /// Repository
    /// <remarks>按服务名和月份分表，e.g.:Logs_CommonService_2022-09</remarks>
    /// </summary>
    private ISugarQueryable<Log> Query(string serviceName)
    {
        var tableName = $"Logs_{serviceName}";
        _sqlSugarClient.CodeFirst.As<Log>(tableName).InitTables(typeof(Log));

        return _sqlSugarClient.Queryable<Log>().AS(tableName);
    }

    /// <summary>
    /// Repository
    /// <remarks>按服务名和月份分表，e.g.:Logs_CommonService_2022-09</remarks>
    /// </summary>
    private SimpleClient<Log> GetRepository()
    {
        return _sqlSugarClient.GetSimpleClient<Log>();
    }

    /// <summary>
    /// 检查日期范围是否合法
    /// </summary>
    /// <param name="dateRange"></param>
    /// <exception cref="ArgumentException"></exception>
    private static void CheckDateRange(ICollection<DateTime> dateRange)
    {
        if (dateRange.Count == 2)
        {
            return;
        }

        throw new ArgumentException("日期范围参数不合法，请传入类似[2022-09-01,2022-09-30]这种格式的数据");
    }

    #endregion
}