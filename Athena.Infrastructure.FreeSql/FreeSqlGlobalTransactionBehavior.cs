using Athena.Infrastructure.EventStorage;
using Athena.Infrastructure.EventStorage.Events;
using Athena.Infrastructure.EventStorage.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Athena.Infrastructure.FreeSql;

/// <summary>
/// FreeSqlGlobalTransactionBehavior
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class FreeSqlGlobalTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly UnitOfWorkManager _unitOfWorkManager;
    private readonly IDomainEventContext _domainEventContext;
    private readonly IIntegrationEventContext _integrationEventContext;
    private readonly IPublisher _publisher;
    private readonly ILogger<FreeSqlGlobalTransactionBehavior<TRequest, TResponse>> _logger;
    private readonly ICapPublisher? _capPublisher;
    private readonly ISecurityContextAccessor? _securityContextAccessor;
    private readonly IOptionsMonitor<EventStorageOptions>? _eventStorageOptions;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="publisher"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="unitOfWorkManager"></param>
    /// <param name="domainEventContext"></param>
    /// <param name="integrationEventContext"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="securityContextAccessor"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public FreeSqlGlobalTransactionBehavior(
        IPublisher publisher,
        ILoggerFactory loggerFactory,
        UnitOfWorkManager unitOfWorkManager,
        IDomainEventContext domainEventContext,
        IIntegrationEventContext integrationEventContext,
        IServiceProvider serviceProvider, ISecurityContextAccessor? securityContextAccessor)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _logger = loggerFactory.CreateLogger<FreeSqlGlobalTransactionBehavior<TRequest, TResponse>>();
        _unitOfWorkManager = unitOfWorkManager;
        _domainEventContext = domainEventContext;
        _integrationEventContext = integrationEventContext;
        _securityContextAccessor = securityContextAccessor;
        _eventStorageOptions = AthenaProvider.GetService<IOptionsMonitor<EventStorageOptions>>();
        _capPublisher = serviceProvider.GetService<ICapPublisher>();
    }

    /// <summary>
    /// 处理
    /// </summary>
    /// <param name="request"></param>
    /// <param name="next"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default
    )
    {
        if (
            request is not ITxTraceRequest<TResponse> &&
            request is not ITxRequest<TResponse> &&
            request is not ITransactionRequest
        )
        {
            return await next();
        }

        var txRequest = request as ITxTraceRequest<TResponse>;
        var rootTraceId = txRequest?.RootTraceId ?? Activity.Current?.TraceId.ToString();

        var freeSqlActivitySource = FreeSqlOTelActivityManager.Instance;
        using var activity = FreeSqlOTelActivityManager.Instance.StartActivity("全局事务处理");
        activity?.SetTag("execute.request", JsonSerializer.Serialize(request));
        activity?.SetTag("execute.request.type", request.GetType().FullName);
        IUnitOfWork? uow;
        ICapTransaction? capTransaction = null;
        using (freeSqlActivitySource.StartActivity("开启事务"))
        {
            uow = _unitOfWorkManager.Begin();
            if (_capPublisher != null)
            {
                capTransaction = _capPublisher.BeginTransaction(uow);
                // capTransaction = uow.BeginTransaction(_capPublisher);
            }
        }

        try
        {
            TResponse? response;
            using (freeSqlActivitySource.StartActivity("执行方法"))
            {
                // 执行方法
                response = await next();
            }

            using (freeSqlActivitySource.StartActivity("领域事件发布"))
            {
                // 领域事件发布处理
                await DomainEventHandleAsync(rootTraceId, cancellationToken);
            }

            using (freeSqlActivitySource.StartActivity("集成事件发布"))
            {
                // 集成事件发布处理
                await IntegrationEventHandleAsync(rootTraceId, cancellationToken);
            }

            using (freeSqlActivitySource.StartActivity("提交事务"))
            {
                // 提交事务
                Commit(capTransaction, uow);
            }

            activity?.SetTag("execute.response", JsonSerializer.Serialize(response));

            return response;
        }
        catch (FriendlyException ex)
        {
            using var errorActivity = freeSqlActivitySource.StartActivity("发生业务异常");
            errorActivity?.SetTag("execute.friendly.exception", ex.Message);
            uow?.Rollback();
            throw;
        }
        catch (DbUpdateVersionException ex)
        {
            using var errorActivity = freeSqlActivitySource.StartActivity("发生数据库异常");
            errorActivity?.SetTag("execute.exception", ex.Message);
            errorActivity?.SetStatus(ActivityStatusCode.Error);
            uow?.Rollback();
            throw FriendlyException.Of("数据已被修改，请刷新后重试");
        }
        catch (Exception ex)
        {
            using var errorActivity = freeSqlActivitySource.StartActivity("发生未知异常");
            errorActivity?.SetTag("execute.exception", ex.Message);
            errorActivity?.SetTag("execute.exception.type", ex.GetType().FullName);
            if (ex.InnerException != null)
            {
                errorActivity?.SetTag("inner.exception", ex.InnerException?.Message ?? string.Empty);
                errorActivity?.SetTag("inner.exception.type", ex.InnerException?.GetType().FullName ?? string.Empty);
            }

            errorActivity?.SetStatus(ActivityStatusCode.Error);
            _logger.LogError(ex, "{Message}", ex.Message);
            uow?.Rollback();
            throw;
        }
        finally
        {
            using (freeSqlActivitySource.StartActivity("事务释放"))
            {
                uow?.Dispose();
            }
        }
    }


    /// <summary>
    /// 领域事件处理
    /// </summary>
    /// <param name="rootTraceId"></param>
    /// <param name="cancellationToken"></param>
    private async Task DomainEventHandleAsync(string? rootTraceId, CancellationToken cancellationToken)
    {
        // 多层领域事件发布处理
        do
        {
            var events = _domainEventContext.GetEvents();
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
    /// <param name="rootTraceId"></param>
    /// <param name="cancellationToken"></param>
    private async Task IntegrationEventHandleAsync(string? rootTraceId, CancellationToken cancellationToken)
    {
        if (_capPublisher == null)
        {
            _logger.LogWarning("集成事件依赖CAP，请配置。services.AddCustomIntegrationEvent(configuration);");
            return;
        }

        var tenantId = TenantId;
        var appId = AppId;
        // 多层集成事件发布处理
        do
        {
            var events = _integrationEventContext.GetEvents();
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
    /// 租户ID
    /// </summary>
    private string? TenantId => _securityContextAccessor?.TenantId;

    /// <summary>
    /// 读取应用ID
    /// </summary>
    private string? AppId => _securityContextAccessor?.AppId;

    /// <summary>
    /// Commit
    /// </summary>
    /// <param name="capTransaction"></param>
    /// <param name="uow"></param>
    private static void Commit(ICapTransaction? capTransaction, IUnitOfWork? uow)
    {
        if (capTransaction == null)
        {
            uow?.Commit();
        }
        else
        {
            capTransaction.Commit();
        }
    }
}