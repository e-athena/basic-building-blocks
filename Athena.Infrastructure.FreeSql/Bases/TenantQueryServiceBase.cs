namespace Athena.Infrastructure.FreeSql.Bases;

/// <summary>
/// 租户的查询服务基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class TenantQueryServiceBase<T> : QueryServiceBase<T> where T : class
{
    private readonly FreeSqlMultiTenancy _multiTenancy;
    private readonly ITenantService _tenantService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="multiTenancy"></param>
    /// <param name="tenantService"></param>
    public TenantQueryServiceBase(FreeSqlMultiTenancy multiTenancy, ITenantService tenantService)
        : base(multiTenancy.Change(Constant.DefaultMainTenant))
    {
        _multiTenancy = multiTenancy;
        _tenantService = tenantService;
    }

    /// <summary>
    /// 切换租户
    /// </summary>
    /// <param name="tenantId">租户ID</param>
    /// <param name="appId">应用Id</param>
    protected void ChangeTenant(string? tenantId, string? appId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            return;
        }

        var exists = _multiTenancy.ExistsRegister(tenantId);
        if (exists)
        {
            SetContext(_multiTenancy.Change(tenantId));
            return;
        }

        var tenant = _tenantService.GetAsync(tenantId, appId).ConfigureAwait(false).GetAwaiter().GetResult();
        if (tenant == null)
        {
            throw new Exception("租户不存在");
        }

        // 注册租户
        // 只会首次注册，如果已经注册过则不生效
        _multiTenancy.Register(tenantId, () =>
            FreeSqlBuilderHelper.Build(
                tenant.ConnectionString,
                tenant.DataType.HasValue ? (DataType) tenant.DataType.Value : null
            )
        );
        // 切换租户
        SetContext(_multiTenancy.Change(tenantId));
    }
}