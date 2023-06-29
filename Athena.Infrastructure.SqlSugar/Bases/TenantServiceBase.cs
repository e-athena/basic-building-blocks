namespace Athena.Infrastructure.SqlSugar.Bases;

/// <summary>
/// 租户服务基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class TenantServiceBase<T> : ServiceBase<T> where T : EntityCore, new()
{
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly ITenantService _tenantService;
    private readonly ILogger _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    /// <param name="tenantService"></param>
    /// <param name="factory"></param>
    public TenantServiceBase(
        ISqlSugarClient sqlSugarClient,
        ITenantService tenantService,
        ILoggerFactory factory
    ) : base(sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
        _tenantService = tenantService;
        _logger = factory.CreateLogger(GetType());
    }

    /// <summary>
    /// 切换租户
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="appId"></param>
    private void ChangeTenant(string? tenantId, string? appId)
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

        var content = SqlSugarBuilderHelper.Registry(
            _sqlSugarClient, tenant.DbKey,
            tenant.ConnectionString,
            tenant.DataType.HasValue ? (DbType) tenant.DataType.Value : null
        );
        SetContext(content);
    }

    /// <summary>
    /// 使用事务
    /// </summary>
    /// <param name="tenantId">租户Id</param>
    /// <param name="appId"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <exception cref="FriendlyException"></exception>
    protected async Task UseTransactionAsync(
        string? tenantId,
        string? appId,
        Func<Task> action
    )
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            tenantId = Constant.DefaultMainTenant;
        }

        ChangeTenant(tenantId, appId);
        using var uow = _sqlSugarClient.CreateContext();
        try
        {
            // 执行方法
            await action();
            uow.Commit();
        }
        catch (FriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            throw;
        }
        finally
        {
            uow.Dispose();
        }
    }
}