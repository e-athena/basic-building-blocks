using Athena.Infrastructure.Messaging.Responses;
using Microsoft.Extensions.Logging;

namespace Athena.Infrastructure.EfCore;

public class QueryServiceBase<T> where T : class
{
    /// <summary>
    /// DbContext
    /// </summary>
    private readonly DbContext _context;

    /// <summary>
    /// ILogger
    /// </summary>
    private readonly ILogger? _logger;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    protected QueryServiceBase(DbContext dbContext)
    {
        _context = dbContext;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="loggerFactory"></param>
    protected QueryServiceBase(DbContext dbContext, ILoggerFactory loggerFactory)
    {
        _context = dbContext;
        _logger = loggerFactory.CreateLogger(typeof(QueryServiceBase<T>));
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected IQueryable<T> Query()
    {
        return _context.Set<T>().AsTracking();
    }

    /// <summary>
    /// [不追踪]查询对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected IQueryable<T> QueryNoTracking()
    {
        return _context.Set<T>().AsNoTracking();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    protected virtual IQueryable<TEntity> Query<TEntity>() where TEntity : class
    {
        return _context.Set<TEntity>().AsTracking();
    }

    /// <summary>
    /// [不追踪]查询对象
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    protected virtual IQueryable<TEntity> QueryNoTracking<TEntity>() where TEntity : class
    {
        return _context.Set<TEntity>().AsNoTracking();
    }

    /// <summary>
    /// sql语句查询
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    protected virtual IQueryable<TEntity> FromSqlRaw<TEntity>(string sql, params object[] parameters)
        where TEntity : class
    {
        return _context.Set<TEntity>().FromSqlRaw(sql, parameters);
    }

    /// <summary>
    /// [异步]通用的执行结果方法
    /// </summary>
    /// <param name="func"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    protected async Task<ApiResult<TResult>> QueryResultAsync<TResult>(Func<Task<TResult>> func)
    {
        var rsp = new ApiResult<TResult>();
        try
        {
            rsp.StatusCode = 200; // 状态码
            rsp.Message = "succeed"; // 信息
            rsp.Success = true; // 是否成功
            rsp.Data = await func(); // 数据集
        }
        catch (FriendlyException ex)
        {
            rsp.Success = false;
            rsp.StatusCode = -1;
            rsp.Message = ex.Message;
        }
        catch (ValidationException ex)
        {
            rsp.Success = false;
            rsp.StatusCode = -2;
            rsp.Message = ex.Message;
        }
        catch (Exception ex)
        {
            if (ex.InnerException is MySqlException exc)
            {
                rsp.Success = false;
                rsp.StatusCode = exc.Number;
                rsp.Message = "MySql数据库异常";
            }

            rsp.Success = false;
            rsp.StatusCode = 500;
            rsp.Message = ex.Message;
            rsp.InnerMessage = ex.InnerException?.Message;
            rsp.StackTrace = ex.StackTrace;

            _logger?.LogError(ex,"Exception Message：{StackTrace}", ex.StackTrace);
        }

        return rsp;
    }

    protected async Task<ApiResult<TResult>> QueryResultAsync<TResult>(Func<Task<TResult>> func, string message)
    {
        var rsp = new ApiResult<TResult>();
        try
        {
            rsp.StatusCode = 200; // 状态码
            rsp.Message = !string.IsNullOrEmpty(message) ? message : "succeed"; // 信息
            rsp.Success = true; // 是否成功
            rsp.Data = await func(); // 数据集
        }
        catch (FriendlyException ex)
        {
            rsp.Success = false;
            rsp.StatusCode = -1;
            rsp.Message = ex.Message;
        }
        catch (ValidationException ex)
        {
            rsp.Success = false;
            rsp.StatusCode = -2;
            rsp.Message = ex.Message;
        }
        catch (Exception ex)
        {
            if (ex.InnerException is MySqlException exc)
            {
                rsp.Success = false;
                rsp.StatusCode = exc.Number;
                rsp.Message = "MySql数据库异常";
            }

            rsp.Success = false;
            rsp.StatusCode = 500;
            rsp.Message = ex.Message;
            rsp.InnerMessage = ex.InnerException?.Message;
            rsp.StackTrace = ex.StackTrace;

            _logger?.LogError(ex,"Exception Message：{StackTrace}", ex.StackTrace);
        }

        return rsp;
    }
}