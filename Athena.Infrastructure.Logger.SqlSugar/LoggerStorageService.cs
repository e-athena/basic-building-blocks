using Athena.Infrastructure.Helpers;
using Athena.Infrastructure.Messaging.Responses;
using Athena.Infrastructure.SqlSugar;

namespace Athena.Infrastructure.Logger.SqlSugar;

/// <summary>
/// 日志存储服务接口实现
/// </summary>
public class LoggerStorageService : ILoggerStorageService
{
    private readonly ISqlSugarLoggerClient _sqlSugarClient;

    // 获取当前月份
    private readonly string _currentMonth = DateTime.Now.ToString("yyyy-MM");

    public LoggerStorageService(ISqlSugarLoggerClient sqlSugarLoggerClient)
    {
        _sqlSugarClient = sqlSugarLoggerClient;
    }

    /// <summary>
    /// 添加日志
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public async Task WriteAsync(Log log)
    {
        var tableName = $"Logs_{log.ServiceName}_{_currentMonth}";
        _sqlSugarClient.CodeFirst.As<Log>(tableName).InitTables(typeof(Log));
        await GetRepository()
            .AsInsertable(log)
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
        return Query(request.ServiceName, request.DateRange)
            .Where(p => p.CreatedOn > request.DateRange[0] && p.CreatedOn < request.DateRange[1].AddDays(1))
            .HasWhere(request.UserId, p => p.UserId == request.UserId)
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
        return Query(request.ServiceName, request.DateRange)
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
        return Query(request.ServiceName, request.DateRange)
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
        var count = await Query(request.ServiceName, request.DateRange)
            .WhereIF(!string.IsNullOrEmpty(request.Route), p => p.Route == request.Route)
            .WhereIF(!string.IsNullOrEmpty(request.UserId), p => p.UserId == request.UserId)
            .WhereIF(!string.IsNullOrEmpty(request.AliasName), p => p.AliasName!.Contains(request.AliasName!))
            .WhereIF(request.StatusCode != null, p => p.StatusCode == request.StatusCode)
            .CountAsync();
        return count;
    }

    #region Private Methods

    /// <summary>
    /// Repository
    /// <remarks>按服务名和月份分表，e.g.:Logs_CommonService_2022-09</remarks>
    /// </summary>
    private ISugarQueryable<Log> Query(string serviceName, IList<DateTime> dateRange)
    {
        CheckDateRange(dateRange);
        var query = _sqlSugarClient.Queryable<Log>();
        var monthList = DateHelper.GetMonthList(dateRange[0], dateRange[1]);
        foreach (var month in monthList)
        {
            var tableName = $"Logs_{serviceName}_{month}";
            _sqlSugarClient.CodeFirst.As<Log>(tableName).InitTables(typeof(Log));
            query = query.AS(tableName);
        }

        return query;
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