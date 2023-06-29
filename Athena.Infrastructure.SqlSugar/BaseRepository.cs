namespace Athena.Infrastructure.SqlSugar;

/// <summary>
/// 仓储基类
/// </summary>
public abstract class BaseRepository
{
    private readonly ISqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    protected BaseRepository(ISqlSugarClient sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected ISugarQueryable<T1> Query<T1>() where T1 : class
    {
        return _sqlSugarClient.Queryable<T1>();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected ISugarQueryable<T1> QueryNoTracking<T1>() where T1 : class
    {
        return Query<T1>();
    }
}