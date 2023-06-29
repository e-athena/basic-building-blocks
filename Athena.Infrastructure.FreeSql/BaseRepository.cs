namespace Athena.Infrastructure.FreeSql;

/// <summary>
/// 仓储基类
/// </summary>
public abstract class BaseRepository
{
    private readonly IFreeSql _freeSql;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="freeSql"></param>
    protected BaseRepository(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected ISelect<T1> Query<T1>() where T1 : class
    {
        return _freeSql.Queryable<T1>();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected ISelect<T1> QueryNoTracking<T1>() where T1 : class
    {
        return Query<T1>().NoTracking();
    }
}