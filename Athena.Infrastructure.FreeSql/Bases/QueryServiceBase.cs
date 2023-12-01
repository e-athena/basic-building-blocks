namespace Athena.Infrastructure.FreeSql.Bases;

/// <summary>
/// 查询服务基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class QueryServiceBase<T> where T : class
{
    /// <summary>
    /// 
    /// </summary>
    protected const string DefaultTenant = "default_tenant";

    /// <summary>
    /// 
    /// </summary>
    protected const string OtherTenant = "other_tenant";

    /// <summary>
    ///
    /// </summary>
    protected const string SoftDelete = "soft_delete";

    private readonly ISecurityContextAccessor? _accessor;
    private readonly FreeSqlMultiTenancy? _multiTenancy;

    /// <summary>
    /// 
    /// </summary>
    protected IFreeSql FreeSqlDbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="freeSql"></param>
    public QueryServiceBase(IFreeSql freeSql)
    {
        FreeSqlDbContext = freeSql;
        if (_multiTenancy != null)
        {
            return;
        }

        _multiTenancy = freeSql as FreeSqlMultiTenancy;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="multiTenancy"></param>
    public QueryServiceBase(FreeSqlMultiTenancy multiTenancy) : this(freeSql: multiTenancy)
    {
        _multiTenancy = multiTenancy;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="freeSql"></param>
    /// <param name="accessor"></param>
    public QueryServiceBase(IFreeSql freeSql, ISecurityContextAccessor accessor) :
        this(freeSql)
    {
        _accessor = accessor;
        GlobalFilterHandler();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="multiTenancy"></param>
    /// <param name="accessor"></param>
    public QueryServiceBase(FreeSqlMultiTenancy multiTenancy, ISecurityContextAccessor accessor) :
        this(multiTenancy)
    {
        _multiTenancy = multiTenancy;
        _accessor = accessor;
        GlobalFilterHandler();
    }

    /// <summary>
    /// 用户ID
    /// </summary>
    protected string? UserId => _accessor?.UserId;

    /// <summary>
    /// 用户名
    /// </summary>
    protected string? UserName => _accessor?.UserName;

    /// <summary>
    /// 用户姓名
    /// </summary>
    protected string? RealName => _accessor?.RealName;

    /// <summary>
    /// 是否为开发者帐号
    /// </summary>
    protected bool IsRoot => _accessor?.IsRoot ?? false;

    /// <summary>
    /// 租户ID
    /// </summary>
    protected string? TenantId => _accessor?.TenantId == "" ? null : _accessor?.TenantId;

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
    protected IFreeSql MainFreeSql
    {
        get
        {
            if (_multiTenancy == null)
            {
                throw new NullReferenceException("FreeSqlMultiTenancy is null.");
            }

            return _multiTenancy.Use("default");
        } //  => _multiTenancy?.Use("default") ??  throw new NullReferenceException("FreeSqlMultiTenancy is null.");
    }

    /// <summary>
    /// 主租户查询
    /// </summary>
    protected ISelect<T> MainQueryableNoTracking
    {
        get
        {
            // 如果T1不是ITenant，就需要禁用过滤器
            if (!typeof(ITenant).IsAssignableFrom(typeof(T)))
            {
                return MainFreeSql
                    .Select<T>()
                    .DisableGlobalFilter(OtherTenant, DefaultTenant)
                    .NoTracking();
            }

            return MainFreeSql
                .Select<T>()
                .DisableGlobalFilter(OtherTenant)
                .NoTracking();
        }
    }

    /// <summary>
    /// 主租户查询
    /// </summary>
    protected ISelect<T1> MainQueryNoTracking<T1>() where T1 : class
    {
        // 如果T1不是ITenant，就需要禁用过滤器
        if (!typeof(ITenant).IsAssignableFrom(typeof(T1)))
        {
            return MainFreeSql
                .Select<T1>()
                .DisableGlobalFilter(OtherTenant, DefaultTenant)
                .NoTracking();
        }

        return MainFreeSql
            .Select<T1>()
            .DisableGlobalFilter(OtherTenant)
            .NoTracking();
    }

    /// <summary>
    /// ASP.NET Core 环境
    /// </summary>
    protected string AspNetCoreEnvironment =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

    /// <summary>
    /// 设置当前上下文
    /// </summary>
    /// <param name="freeSql"></param>
    protected void SetContext(IFreeSql freeSql)
    {
        FreeSqlDbContext = freeSql;
        GlobalFilterHandler();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISelect<T> Queryable => FreeSqlDbContext.Select<T>();

    /// <summary>
    /// 查询对象
    /// <remarks>软删除的数据也会被查询出来</remarks>
    /// </summary>
    protected virtual ISelect<T> QueryableWithSoftDelete => Queryable.DisableGlobalFilter(SoftDelete);

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISelect<T> QueryableNoTracking => Queryable.NoTracking();

    /// <summary>
    /// 查询对象
    /// <remarks>软删除的数据也会被查询出来</remarks>
    /// </summary>
    protected virtual ISelect<T> QueryableNoTrackingWithSoftDelete =>
        QueryableNoTracking.DisableGlobalFilter(SoftDelete);

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISelect<T> Query()
    {
        return FreeSqlDbContext.Queryable<T>();
    }

    /// <summary>
    /// 查询对象
    /// <remarks>软删除的数据也会被查询出来</remarks>
    /// </summary>
    protected virtual ISelect<T> QueryWithSoftDelete()
    {
        return Query().DisableGlobalFilter(SoftDelete);
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
    /// <remarks>软删除的数据也会被查询出来</remarks>
    /// </summary>
    protected virtual ISelect<T> QueryNoTrackingWithSoftDelete()
    {
        return QueryNoTracking().DisableGlobalFilter(SoftDelete);
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISelect<T1> Query<T1>() where T1 : class
    {
        // 如果T1不是ITenant，就需要禁用过滤器
        if (!typeof(ITenant).IsAssignableFrom(typeof(T1)))
        {
            return FreeSqlDbContext
                .Queryable<T1>()
                .DisableGlobalFilter(OtherTenant, DefaultTenant);
        }

        return FreeSqlDbContext.Queryable<T1>();
    }

    /// <summary>
    /// 查询对象
    /// <remarks>软删除的数据也会被查询出来</remarks>
    /// </summary>
    protected virtual ISelect<T1> QueryWithSoftDelete<T1>() where T1 : class
    {
        return Query<T1>().DisableGlobalFilter(SoftDelete);
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    protected virtual ISelect<T1> QueryNoTracking<T1>() where T1 : class
    {
        return Query<T1>().NoTracking();
    }

    /// <summary>
    /// 查询对象
    /// <remarks>软删除的数据也会被查询出来</remarks>
    /// </summary>
    protected virtual ISelect<T1> QueryNoTrackingWithSoftDelete<T1>() where T1 : class
    {
        return QueryNoTracking<T1>().DisableGlobalFilter(SoftDelete);
    }

    // 全局过滤器处理
    private void GlobalFilterHandler()
    {
        FreeSqlDbContext
            .GlobalFilter
            .ApplyIf<ITenant>(
                DefaultTenant,
                () => string.IsNullOrEmpty(TenantId),
                p => string.IsNullOrEmpty(p.TenantId),
                true
            )
            .ApplyIf<ITenant>(
                OtherTenant,
                () => !string.IsNullOrEmpty(TenantId),
                p => p.TenantId == TenantId,
                true
            )
            .Apply<ISoftDelete>(
                SoftDelete,
                p => !p.IsDeleted,
                true
            );
    }
}