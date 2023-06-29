namespace Athena.Infrastructure.FreeSql.Bases;

/// <summary>
/// 查询服务基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class QueryServiceBase<T> where T : class
{
    private  IFreeSql _freeSql;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="freeSql"></param>
    public QueryServiceBase(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }
    
    /// <summary>
    /// 设置当前上下文
    /// </summary>
    /// <param name="freeSql"></param>
    protected void SetContext(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISelect<T> Queryable => _freeSql.Select<T>();

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISelect<T> QueryableNoTracking => Queryable.NoTracking();

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISelect<T> Query()
    {
        return _freeSql.Queryable<T>();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISelect<T> QueryNoTracking()
    {
        return Query().NoTracking();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISelect<T1> Query<T1>() where T1 : class
    {
        return _freeSql.Queryable<T1>();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISelect<T1> QueryNoTracking<T1>() where T1 : class
    {
        return Query<T1>().NoTracking();
    }
}