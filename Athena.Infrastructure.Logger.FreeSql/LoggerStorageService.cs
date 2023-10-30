using Athena.Infrastructure.Helpers;
using Athena.Infrastructure.Messaging.Responses;

namespace Athena.Infrastructure.Logger.FreeSql;

/// <summary>
/// 日志存储服务接口实现
/// </summary>
public class LoggerStorageService : ILoggerStorageService
{
    private readonly IFreeSql<ILoggerFreeSql> _freeSql;

    // 获取当前月份
    private readonly string _currentMonth = DateTime.Now.ToString("yyyy-MM");

    /// <summary>
    /// 
    /// </summary>
    /// <param name="freeSql"></param>
    public LoggerStorageService(IFreeSql<ILoggerFreeSql> freeSql)
    {
        _freeSql = freeSql;
    }

    /// <summary>
    /// 添加日志
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public async Task WriteAsync(Log log)
    {
        await GetRepository(log.ServiceName).InsertAsync(log);
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
            .FirstAsync<GetByIdResponse>();
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
        return Query(request.ServiceName, request.DateRange)
            .WhereIf(!string.IsNullOrEmpty(request.Route), p => p.Route == request.Route)
            .WhereIf(!string.IsNullOrEmpty(request.UserId), p => p.UserId == request.UserId)
            .WhereIf(!string.IsNullOrEmpty(request.AliasName), p => p.AliasName!.Contains(request.AliasName!))
            .WhereIf(request.StatusCode != null, p => p.StatusCode == request.StatusCode)
            .CountAsync();
    }

    #region Private Methods

    /// <summary>
    /// Repository
    /// <remarks>按服务名和月份分表，e.g.:Logs_CommonService_2022-09</remarks>
    /// </summary>
    private ISelect<Log> Query(string serviceName, IList<DateTime> dateRange)
    {
        CheckDateRange(dateRange);
        var query = _freeSql.Select<Log>().NoTracking();
        var monthList = DateHelper.GetMonthList(dateRange[0], dateRange[1]);
        foreach (var month in monthList)
        {
            query = query.AsTable((_, oldName) => $"{oldName}_{serviceName}_{month}");
        }

        return query;
    }

    /// <summary>
    /// Repository
    /// <remarks>按服务名和月份分表，e.g.:Logs_CommonService_2022-09</remarks>
    /// </summary>
    private IBaseRepository<Log, long> GetRepository(string serviceName, string? month = null)
    {
        // 检查月份是否合法
        CheckMonth(month);
        var resp = _freeSql.GetRepository<Log, long>();
        resp.AsTable(oldName => $"{oldName}_{serviceName}_{month ?? _currentMonth}");
        return resp;
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

    /// <summary>
    /// 检查月份是否合法
    /// </summary>
    /// <param name="month"></param>
    /// <exception cref="ArgumentException"></exception>
    private static void CheckMonth(string? month)
    {
        if (month == null)
        {
            return;
        }

        const string errorMessage = "月份参数不合法，请传入类似[2022-09]这种格式的数据";
        if (month.Length != 7)
        {
            throw new ArgumentException(errorMessage);
        }

        if (DateTime.TryParse(month, out _))
        {
            return;
        }

        throw new ArgumentException(errorMessage);
    }

    #endregion
}