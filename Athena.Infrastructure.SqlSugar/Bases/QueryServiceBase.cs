namespace Athena.Infrastructure.SqlSugar.Bases;

/// <summary>
/// 查询服务基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class QueryServiceBase<T> where T : class, new()
{
    private ISqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    public QueryServiceBase(ISqlSugarClient sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 设置当前上下文
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    protected void SetContext(ISqlSugarClient sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISugarQueryable<T> Queryable => _sqlSugarClient.Queryable<T>();

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISugarQueryable<T> QueryableNoTracking => Queryable;

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISugarQueryable<T> Query()
    {
        return _sqlSugarClient.Queryable<T>();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISugarQueryable<T> QueryNoTracking()
    {
        return Query();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISugarQueryable<T1> Query<T1>() where T1 : class, new()
    {
        return _sqlSugarClient.Queryable<T1>();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISugarQueryable<T1> QueryNoTracking<T1>() where T1 : class, new()
    {
        return Query<T1>();
    }
}