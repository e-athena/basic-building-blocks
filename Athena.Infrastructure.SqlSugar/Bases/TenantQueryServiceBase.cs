namespace Athena.Infrastructure.SqlSugar.Bases;

/// <summary>
/// 租户的查询服务基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class TenantQueryServiceBase<T> : QueryServiceBase<T> where T : class, new()
{
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly ITenantService _tenantService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    /// <param name="tenantService"></param>
    public TenantQueryServiceBase(ISqlSugarClient sqlSugarClient, ITenantService tenantService)
        : base(sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
        _tenantService = tenantService;
    }

    /// <summary>
    /// 切换租户
    /// </summary>
    /// <param name="eventBase"></param>
    protected void ChangeTenant(EventBase eventBase)
    {
        ChangeTenant(eventBase.TenantId, eventBase.AppId);
    }

    /// <summary>
    /// 切换租户
    /// </summary>
    /// <param name="tenantId">租户Id</param>
    /// <param name="appId">应用ID</param>
    protected void ChangeTenant(string? tenantId, string? appId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            return;
        }

        var exists = _sqlSugarClient.AsTenant().IsAnyConnection(tenantId);
        if (exists)
        {
            SetContext(_sqlSugarClient.AsTenant().GetConnectionScope(tenantId));
            return;
        }

        var tenant = _tenantService.GetAsync(tenantId, appId).ConfigureAwait(false).GetAwaiter().GetResult();
        if (tenant == null)
        {
            throw new Exception("租户不存在");
        }

        var content =
            SqlSugarBuilderHelper.Registry(
                _sqlSugarClient,
                tenant.DbKey,
                tenant.ConnectionString,
                tenant.DataType.HasValue ? (DbType) tenant.DataType.Value : null
            );
        SetContext(content);
    }
}