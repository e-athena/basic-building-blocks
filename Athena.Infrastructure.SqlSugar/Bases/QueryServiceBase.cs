using ITenant = Athena.Infrastructure.Domain.ITenant;

namespace Athena.Infrastructure.SqlSugar.Bases;

/// <summary>
/// 查询服务基类
/// </summary>
/// <typeparam name="T">实体类</typeparam>
public class QueryServiceBase<T> where T : class, new()
{
    private string? _tenantId;

    private string? _userId;

    private string? _userName;

    private string? _realName;

    private readonly ISecurityContextAccessor? _accessor;

    /// <summary>
    /// 当前上下文
    /// </summary>
    protected ISqlSugarClient DbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    public QueryServiceBase(ISqlSugarClient sqlSugarClient)
    {
        DbContext = sqlSugarClient;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    /// <param name="accessor"></param>
    public QueryServiceBase(ISqlSugarClient sqlSugarClient, ISecurityContextAccessor accessor) :
        this(sqlSugarClient)
    {
        _accessor = accessor;
        GlobalFilterHandler();
    }

    /// <summary>
    /// 用户ID
    /// </summary>
    protected string? UserId => _userId ?? _accessor?.UserId;

    /// <summary>
    /// 设置用户ID
    /// </summary>
    /// <param name="userId"></param>
    protected void SetUserId(string? userId)
    {
        _userId = userId;
    }

    /// <summary>
    /// 用户名
    /// </summary>
    protected string? UserName => _accessor?.UserName;

    /// <summary>
    /// 设置用户名
    /// </summary>
    /// <param name="userName"></param>
    protected void SetUserName(string? userName)
    {
        _userName = userName;
    }

    /// <summary>
    /// 用户姓名
    /// </summary>
    protected string? RealName => _realName ?? _accessor?.RealName;

    /// <summary>
    /// 设置用户姓名
    /// </summary>
    /// <param name="realName"></param>
    protected void SetRealName(string? realName)
    {
        _realName = realName;
    }

    /// <summary>
    /// 是否为开发者帐号
    /// </summary>
    protected bool IsRoot =>
        !string.IsNullOrEmpty(_userName) && _userName == "root" || (_accessor?.IsRoot ?? false);

    /// <summary>
    /// 租户ID
    /// </summary>
    protected string? TenantId => _tenantId ?? (_accessor?.TenantId == "" ? null : _accessor?.TenantId);

    /// <summary>
    /// 设置租户ID
    /// </summary>
    /// <param name="tenantId"></param>
    protected void SetTenantId(string? tenantId)
    {
        _tenantId = tenantId;
        GlobalFilterHandler();
    }

    /// <summary>
    /// 是否为租户管理员
    /// </summary>
    protected bool IsTenantAdmin => _accessor?.IsTenantAdmin ?? false;

    /// <summary>
    /// 是否租户环境
    /// </summary>
    protected bool IsTenantEnvironment => !string.IsNullOrEmpty(TenantId);

    /// <summary>
    /// 组织架构
    /// </summary>
    protected virtual string? OrganizationalUnitIds => null;

    /// <summary>
    /// 主租户
    /// </summary>
    protected ISqlSugarClient MainFreeSql => DbContext.AsTenant().GetConnectionScope("default");

    /// <summary>
    /// 主租户查询
    /// </summary>
    protected ISugarQueryable<T> MainQueryableNoTracking
    {
        get
        {
            // 如果T1不是ITenant，就需要禁用过滤器
            if (!typeof(ITenant).IsAssignableFrom(typeof(T)))
            {
                return MainFreeSql
                    .Queryable<T>()
                    .ClearFilter<ITenant>();
            }

            return MainFreeSql.Queryable<T>();
        }
    }


    /// <summary>
    /// 主租户查询
    /// </summary>
    protected ISugarQueryable<T1> MainQueryNoTracking<T1>() where T1 : class
    {
        // 如果T1不是ITenant，就需要禁用过滤器
        if (!typeof(ITenant).IsAssignableFrom(typeof(T1)))
        {
            return MainFreeSql
                .Queryable<T1>()
                .ClearFilter<ITenant>();
        }

        return MainFreeSql.Queryable<T1>();
    }

    /// <summary>
    /// ASP.NET Core 环境
    /// </summary>
    protected string AspNetCoreEnvironment =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

    /// <summary>
    /// 设置当前上下文
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    protected void SetContext(ISqlSugarClient sqlSugarClient)
    {
        DbContext = sqlSugarClient;
        GlobalFilterHandler();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISugarQueryable<T> Queryable => DbContext.Queryable<T>();

    /// <summary>
    /// 查询对象
    /// <remarks>软删除的数据也会被查询出来</remarks>
    /// </summary>
    protected virtual ISugarQueryable<T> QueryableWithSoftDelete => Queryable.ClearFilter<ISoftDelete>();

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISugarQueryable<T> QueryableNoTracking => Queryable;

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISugarQueryable<T> QueryableNoTrackingWithSoftDelete =>
        QueryableNoTracking.ClearFilter<ISoftDelete>();

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISugarQueryable<T> Query()
    {
        return DbContext.Queryable<T>();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISugarQueryable<T> QueryWithSoftDelete()
    {
        return Query().ClearFilter<ISoftDelete>();
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
    protected virtual ISugarQueryable<T> QueryNoTrackingWithSoftDelete()
    {
        return QueryNoTracking().ClearFilter<ISoftDelete>();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISugarQueryable<T1> Query<T1>() where T1 : class, new()
    {
        // 如果T1不是ITenant，就需要禁用过滤器
        if (!typeof(ITenant).IsAssignableFrom(typeof(T1)))
        {
            return DbContext
                .Queryable<T1>()
                .ClearFilter<ITenant>();
        }

        return DbContext.Queryable<T1>();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISugarQueryable<T1> QueryWithSoftDelete<T1>() where T1 : class, new()
    {
        return Query<T1>().ClearFilter<ISoftDelete>();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISugarQueryable<T1> QueryNoTracking<T1>() where T1 : class, new()
    {
        return Query<T1>();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISugarQueryable<T1> QueryNoTrackingWithSoftDelete<T1>() where T1 : class, new()
    {
        return QueryNoTracking<T1>().ClearFilter<ISoftDelete>();
    }

    // 全局过滤器处理
    private void GlobalFilterHandler()
    {
        DbContext
            .QueryFilter
            .AddTableFilterIF<ITenant>(
                string.IsNullOrEmpty(TenantId),
                p => string.IsNullOrEmpty(p.TenantId),
                QueryFilterProvider.FilterJoinPosition.Where
            )
            .AddTableFilterIF<ITenant>(
                !string.IsNullOrEmpty(TenantId),
                p => p.TenantId == TenantId,
                QueryFilterProvider.FilterJoinPosition.Where
            )
            .AddTableFilter<ISoftDelete>(
                p => p.IsDeleted == false,
                QueryFilterProvider.FilterJoinPosition.Where
            );
    }
}