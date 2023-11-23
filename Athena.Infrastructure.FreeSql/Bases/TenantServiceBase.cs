using System.Data;

namespace Athena.Infrastructure.FreeSql.Bases;

/// <summary>
/// 租户服务基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class TenantServiceBase<T> : ServiceBase<T> where T : EntityCore, new()
{
    private readonly UnitOfWorkManagerCloud _cloud;
    private readonly ITenantService _tenantService;
    private readonly ILogger _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cloud"></param>
    /// <param name="tenantService"></param>
    /// <param name="factory"></param>
    public TenantServiceBase(
        UnitOfWorkManagerCloud cloud,
        ITenantService tenantService,
        ILoggerFactory factory
    ) : base(cloud.GetUnitOfWorkManager(Constant.DefaultMainTenant))
    {
        _cloud = cloud;
        _tenantService = tenantService;
        _logger = factory.CreateLogger(GetType());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cloud"></param>
    /// <param name="tenantService"></param>
    /// <param name="factory"></param>
    /// <param name="accessor"></param>
    public TenantServiceBase(
        UnitOfWorkManagerCloud cloud,
        ITenantService tenantService,
        ILoggerFactory factory,
        ISecurityContextAccessor accessor
    ) : base(cloud.GetUnitOfWorkManager(Constant.DefaultMainTenant), accessor)
    {
        _cloud = cloud;
        _tenantService = tenantService;
        _logger = factory.CreateLogger(GetType());
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
    /// <param name="tenantId">租户ID</param>
    /// <param name="appId">应用ID</param>
    private void ChangeTenant(string? tenantId, string? appId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            return;
        }

        var exists = _cloud.MultiTenancy.ExistsRegister(tenantId);
        if (exists)
        {
            _cloud.MultiTenancy.Change(tenantId);
            SetUnitOfWorkManager(_cloud.GetUnitOfWorkManager(tenantId));
            return;
        }

        var tenant = _tenantService.GetAsync(tenantId, appId).ConfigureAwait(false).GetAwaiter().GetResult();
        if (tenant == null)
        {
            throw new Exception("租户不存在");
        }

        // 注册租户
        // 只会首次注册，如果已经注册过则不生效
        _cloud.MultiTenancy.Register(tenantId, () =>
            FreeSqlBuilderHelper.Build(tenant.ConnectionString)
        );
        _cloud.MultiTenancy.Change(tenantId);
        SetUnitOfWorkManager(_cloud.GetUnitOfWorkManager(tenantId));
    }

    /// <summary>
    /// 使用事务
    /// </summary>
    /// <param name="eventBase"></param>
    /// <param name="action"></param>
    /// <param name="propagation"></param>
    /// <param name="isolationLevel"></param>
    /// <returns></returns>
    /// <exception cref="FriendlyException"></exception>
    protected Task UseTransactionAsync(
        EventBase eventBase,
        Func<Task> action,
        Propagation propagation = Propagation.Required,
        IsolationLevel? isolationLevel = null
    )
    {
        return UseTransactionAsync(eventBase.TenantId, eventBase.AppId, action, propagation, isolationLevel);
    }

    /// <summary>
    /// 使用事务
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="appId"></param>
    /// <param name="action"></param>
    /// <param name="propagation"></param>
    /// <param name="isolationLevel"></param>
    /// <returns></returns>
    /// <exception cref="FriendlyException"></exception>
    protected async Task UseTransactionAsync(
        string? tenantId,
        string? appId,
        Func<Task> action,
        Propagation propagation = Propagation.Required,
        IsolationLevel? isolationLevel = null
    )
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            tenantId = Constant.DefaultMainTenant;
        }

        ChangeTenant(tenantId, appId);
        var uow = _cloud.Begin(tenantId, propagation, isolationLevel);
        try
        {
            // 执行方法
            await action();
            uow.Commit();
        }
        catch (FriendlyException ex)
        {
            _logger.LogInformation(ex,
                "{Type},{Method},{Message}",
                GetType().Name,
                nameof(UseTransactionAsync),
                ex.Message
            );
            uow.Rollback();
            throw;
        }
        catch (DbUpdateVersionException ex)
        {
            _logger.LogInformation(ex,
                "{Type},{Method},{Message}",
                GetType().Name,
                nameof(UseTransactionAsync),
                ex.Message
            );
            uow.Rollback();
            throw FriendlyException.Of("数据已被修改，请刷新后重试");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "{Type},{Method},{Message}",
                GetType().Name,
                nameof(UseTransactionAsync),
                ex.Message
            );
            uow.Rollback();
            throw;
        }
        finally
        {
            uow.Dispose();
        }
    }
}