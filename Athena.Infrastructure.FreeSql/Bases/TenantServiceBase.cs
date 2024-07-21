using Athena.Infrastructure.EventStorage;
using Athena.Infrastructure.EventStorage.Events;
using Athena.Infrastructure.EventStorage.Models;
using Athena.Infrastructure.FreeSql.EventContexts;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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
    private readonly ICapPublisher? _capPublisher;
    private readonly IPublisher? _publisher;
    private readonly IOptionsMonitor<EventStorageOptions>? _eventStorageOptions;

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
        _capPublisher = AthenaProvider.GetService<ICapPublisher>();
        _eventStorageOptions = AthenaProvider.GetService<IOptionsMonitor<EventStorageOptions>>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cloud"></param>
    /// <param name="tenantService"></param>
    /// <param name="factory"></param>
    /// <param name="publisher"></param>
    public TenantServiceBase(
        UnitOfWorkManagerCloud cloud,
        ITenantService tenantService,
        ILoggerFactory factory,
        IPublisher publisher
    ) : base(cloud.GetUnitOfWorkManager(Constant.DefaultMainTenant))
    {
        _cloud = cloud;
        _tenantService = tenantService;
        _logger = factory.CreateLogger(GetType());
        _publisher = publisher;
        _capPublisher = AthenaProvider.GetService<ICapPublisher>();
        _eventStorageOptions = AthenaProvider.GetService<IOptionsMonitor<EventStorageOptions>>();
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
        _capPublisher = AthenaProvider.GetService<ICapPublisher>();
        _eventStorageOptions = AthenaProvider.GetService<IOptionsMonitor<EventStorageOptions>>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cloud"></param>
    /// <param name="tenantService"></param>
    /// <param name="factory"></param>
    /// <param name="accessor"></param>
    /// <param name="publisher"></param>
    public TenantServiceBase(
        UnitOfWorkManagerCloud cloud,
        ITenantService tenantService,
        ILoggerFactory factory,
        ISecurityContextAccessor accessor,
        IPublisher publisher
    ) : base(cloud.GetUnitOfWorkManager(Constant.DefaultMainTenant), accessor)
    {
        _cloud = cloud;
        _tenantService = tenantService;
        _logger = factory.CreateLogger(GetType());
        _publisher = publisher;
        _capPublisher = AthenaProvider.GetService<ICapPublisher>();
        _eventStorageOptions = AthenaProvider.GetService<IOptionsMonitor<EventStorageOptions>>();
    }

    /// <summary>
    /// 切换租户
    /// </summary>
    /// <param name="eventBase"></param>
    protected void ChangeTenant(EventBase eventBase)
    {
        ChangeTenant(eventBase.TenantId, eventBase.AppId);
        SetUserId(eventBase.GetUserId());
        SetUserName(eventBase.GetUserName());
        SetRealName(eventBase.GetRealName());
    }

    /// <summary>
    /// 切换租户
    /// </summary>
    /// <param name="tenantId">租户ID</param>
    /// <param name="appId">应用ID</param>
    protected void ChangeTenant(string? tenantId, string? appId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            return;
        }

        var exists = _cloud.MultiTenancy.ExistsRegister(tenantId);
        if (exists)
        {
            if (tenantId != Constant.DefaultMainTenant)
            {
                SetTenantId(tenantId);
            }

            _cloud.MultiTenancy.Change(tenantId);
            SetUnitOfWorkManager(_cloud.GetUnitOfWorkManager(tenantId));
            return;
        }

        var tenant = _tenantService.GetAsync(tenantId, appId).ConfigureAwait(false).GetAwaiter().GetResult();
        if (tenant == null)
        {
            throw new Exception("租户不存在");
        }

        // 共享租户, 使用默认连接字符串
        if (tenant.IsolationLevel == TenantIsolationLevel.Shared)
        {
            tenant.ConnectionString = _cloud.MultiTenancy.Ado.ConnectionString;
        }

        if (tenantId != Constant.DefaultMainTenant)
        {
            SetTenantId(tenantId);
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
        return UseTransactionAsync(
            eventBase.TenantId,
            eventBase.AppId,
            action,
            eventBase.RootTraceId,
            propagation,
            isolationLevel
        );
    }

    // /// <summary>
    // /// 使用事务
    // /// </summary>
    // /// <param name="tenantId"></param>
    // /// <param name="appId"></param>
    // /// <param name="action"></param>
    // /// <param name="propagation"></param>
    // /// <param name="isolationLevel"></param>
    // /// <returns></returns>
    // /// <exception cref="FriendlyException"></exception>
    // protected async Task UseTransactionAsync(
    //     string? tenantId,
    //     string? appId,
    //     Func<Task> action,
    //     Propagation propagation = Propagation.Required,
    //     IsolationLevel? isolationLevel = null
    // )
    // {
    //     if (string.IsNullOrEmpty(tenantId))
    //     {
    //         tenantId = Constant.DefaultMainTenant;
    //     }
    //
    //     ChangeTenant(tenantId, appId);
    //     var uow = _cloud.Begin(tenantId, propagation, isolationLevel);
    //     try
    //     {
    //         // 执行方法
    //         await action();
    //         uow.Commit();
    //     }
    //     catch (FriendlyException ex)
    //     {
    //         _logger.LogInformation(ex,
    //             "{Type},{Method},{Message}",
    //             GetType().Name,
    //             nameof(UseTransactionAsync),
    //             ex.Message
    //         );
    //         uow.Rollback();
    //         throw;
    //     }
    //     catch (DbUpdateVersionException ex)
    //     {
    //         _logger.LogInformation(ex,
    //             "{Type},{Method},{Message}",
    //             GetType().Name,
    //             nameof(UseTransactionAsync),
    //             ex.Message
    //         );
    //         uow.Rollback();
    //         throw FriendlyException.Of("数据已被修改，请刷新后重试");
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex,
    //             "{Type},{Method},{Message}",
    //             GetType().Name,
    //             nameof(UseTransactionAsync),
    //             ex.Message
    //         );
    //         uow.Rollback();
    //         throw;
    //     }
    //     finally
    //     {
    //         uow.Dispose();
    //     }
    // }

    /// <summary>
    /// 使用事务
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="appId"></param>
    /// <param name="action"></param>
    /// <param name="rootTraceId"></param>
    /// <param name="propagation"></param>
    /// <param name="isolationLevel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="FriendlyException"></exception>
    protected async Task UseTransactionAsync(
        string? tenantId,
        string? appId,
        Func<Task> action,
        string? rootTraceId = null,
        Propagation propagation = Propagation.Required,
        IsolationLevel? isolationLevel = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            tenantId = Constant.DefaultMainTenant;
        }

        ICapTransaction? capTransaction = null;
        ChangeTenant(tenantId, appId);
        var uow = _cloud.Begin(tenantId, propagation, isolationLevel);
        if (_capPublisher != null)
        {
            capTransaction = _capPublisher.BeginTransaction(uow);
        }

        try
        {
            // 执行方法
            await action();
            // 领域事件发布处理
            await DomainEventHandleAsync(_cloud.GetUnitOfWorkManager(tenantId), rootTraceId, cancellationToken);
            // 集成事件发布处理
            await IntegrationEventHandleAsync(_cloud.GetUnitOfWorkManager(tenantId), appId, rootTraceId,
                cancellationToken);
            // 提交事务
            Commit(capTransaction, uow);
        }
        catch (FriendlyException ex)
        {
            uow.Rollback();
            _logger.LogInformation(ex, "{Message}", ex.Message);
            throw;
        }
        catch (DbUpdateVersionException ex)
        {
            uow.Rollback();
            _logger.LogInformation(ex, "{Message}", ex.Message);
            throw FriendlyException.Of("数据已被修改，请刷新后重试");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            uow.Rollback();
            throw;
        }
        finally
        {
            uow.Dispose();
        }
    }

    /// <summary>
    /// 领域事件处理
    /// </summary>
    /// <param name="uow"></param>
    /// <param name="rootTraceId"></param>
    /// <param name="cancellationToken"></param>
    private async Task DomainEventHandleAsync(UnitOfWorkManager uow, string? rootTraceId,
        CancellationToken cancellationToken)
    {
        if (_publisher == null)
        {
            return;
        }

        // 多层领域事件发布处理
        do
        {
            var events = new DomainEventContext(uow).GetEvents();
            if (!events.Any())
            {
                break;
            }

            foreach (var @event in events)
            {
                // publish
                @event.RootTraceId ??= rootTraceId;
                @event.RootTraceId ??= Guid.NewGuid().ToString("N");
                await _publisher.Publish(@event, cancellationToken);
            }
        } while (true);
    }

    /// <summary>
    /// 集成事件处理
    /// <remarks>集成事件依赖CAP</remarks>
    /// </summary>
    /// <param name="uow"></param>
    /// <param name="appId"></param>
    /// <param name="rootTraceId"></param>
    /// <param name="cancellationToken"></param>
    private async Task IntegrationEventHandleAsync(UnitOfWorkManager uow, string? appId, string? rootTraceId,
        CancellationToken cancellationToken)
    {
        if (_capPublisher == null)
        {
            _logger.LogWarning("集成事件依赖CAP，请配置。services.AddCustomIntegrationEvent(configuration);");
            return;
        }

        var tenantId = TenantId;
        // 多层集成事件发布处理
        do
        {
            var events = new IntegrationEventContext(uow).GetEvents();
            if (!events.Any())
            {
                break;
            }

            foreach (var @event in events)
            {
                @event.TenantId = tenantId;
                @event.AppId = appId;
                @event.RootTraceId ??= rootTraceId;
                @event.RootTraceId ??= Guid.NewGuid().ToString("N");
                await _capPublisher.PublishAsync(
                    @event.EventName,
                    @event,
                    @event.CallbackName,
                    cancellationToken
                );

                // 不启用事件存储
                if (_eventStorageOptions == null || !_eventStorageOptions.CurrentValue.Enabled)
                {
                    continue;
                }

                try
                {
                    // 事件存储发布，用于事件回溯，异步执行
                    @event.MetaData.TryGetValue("entityTypeName", out var entityTypeName);
                    @event.MetaData.TryGetValue("version", out var version);
                    @event.MetaData.TryGetValue("userId", out var userId);
                    await _capPublisher.PublishAsync(
                        StringHelper.ConvertToLowerAndAddPoint(nameof(EventPublished)),
                        new EventPublished(new EventStream
                        {
                            AggregateRootTypeName = entityTypeName?.ToString() ?? @event.GetType().Name,
                            AggregateRootId = @event.GetId()!,
                            Version = int.Parse(version?.ToString() ?? "0"),
                            EventId = @event.EventId,
                            EventName = @event.EventName,
                            CreatedOn = @event.CreatedOn,
                            Events = JsonConvert.SerializeObject(@event),
                            UserId = userId?.ToString()
                        }),
                        cancellationToken: cancellationToken
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "事件存储发布失败");
                }
            }
        } while (true);
    }

    /// <summary>
    /// Commit
    /// </summary>
    /// <param name="capTransaction"></param>
    /// <param name="uow"></param>
    private static void Commit(ICapTransaction? capTransaction, IUnitOfWork uow)
    {
        if (capTransaction == null)
        {
            uow.Commit();
        }
        else
        {
            capTransaction.Commit();
        }
    }
}