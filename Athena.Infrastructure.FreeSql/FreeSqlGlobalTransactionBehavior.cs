using System.Diagnostics;
using System.Text.Json;
using Athena.Infrastructure.Event.IntegrationEvents;
using Microsoft.AspNetCore.Http;

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
    private readonly IMediator _mediator;
    private readonly ILogger<FreeSqlGlobalTransactionBehavior<TRequest, TResponse>> _logger;
    private readonly ICapPublisher? _capPublisher;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="unitOfWorkManager"></param>
    /// <param name="domainEventContext"></param>
    /// <param name="integrationEventContext"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="httpContextAccessor"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public FreeSqlGlobalTransactionBehavior(
        IMediator mediator,
        ILoggerFactory loggerFactory,
        UnitOfWorkManager unitOfWorkManager,
        IDomainEventContext domainEventContext,
        IIntegrationEventContext integrationEventContext,
        IServiceProvider serviceProvider, IHttpContextAccessor? httpContextAccessor)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = loggerFactory.CreateLogger<FreeSqlGlobalTransactionBehavior<TRequest, TResponse>>();
        _unitOfWorkManager = unitOfWorkManager;
        _domainEventContext = domainEventContext;
        _integrationEventContext = integrationEventContext;
        _httpContextAccessor = httpContextAccessor;
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
        if (request is not ITransactionRequest)
        {
            return await next();
        }

        var freeSqlActivitySource = FreeSqlOTelActivityManager.Instance;
        using var activity = FreeSqlOTelActivityManager.Instance.StartActivity("全局事务处理");
        activity?.SetTag("execute.request", JsonSerializer.Serialize(request));
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
                await DomainEventHandleAsync(cancellationToken);
            }

            using (freeSqlActivitySource.StartActivity("集成事件发布"))
            {
                // 集成事件发布处理
                await IntegrationEventHandleAsync(cancellationToken);
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
            errorActivity?.SetTag("inner.exception", ex.InnerException?.Message ?? string.Empty);
            errorActivity?.SetTag("inner.exception.type", ex.InnerException?.GetType().FullName ?? string.Empty);
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
    /// <param name="cancellationToken"></param>
    private async Task DomainEventHandleAsync(CancellationToken cancellationToken)
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
                await _mediator.Publish(@event, cancellationToken);
            }
        } while (true);
    }

    /// <summary>
    /// 集成事件处理
    /// <remarks>集成事件依赖CAP</remarks>
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task IntegrationEventHandleAsync(CancellationToken cancellationToken)
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
                await _capPublisher.PublishAsync(
                    @event.EventName,
                    @event,
                    @event.CallbackName,
                    cancellationToken
                );
            }
        } while (true);
    }

    /// <summary>
    /// 租户ID
    /// </summary>
    private string? TenantId
    {
        get
        {
            if (_httpContextAccessor == null)
            {
                return null;
            }

            var context = _httpContextAccessor.HttpContext;
            var tenantId = context.Request.Headers["TenantId"];
            if (!string.IsNullOrEmpty(tenantId))
            {
                return tenantId;
            }

            tenantId = context.Request.Query["tenant_id"].ToString();
            return (!string.IsNullOrEmpty(tenantId)
                ? tenantId.ToString()
                : context.User.FindFirst("TenantId")?.Value) ?? null;
        }
    }
    
    /// <summary>
    /// 读取应用ID
    /// </summary>
    private string? AppId
    {
        get
        {
            if (_httpContextAccessor == null)
            {
                return null;
            }

            var context = _httpContextAccessor.HttpContext;
            var appId = context.Request.Headers["AppId"];
            if (!string.IsNullOrEmpty(appId))
            {
                return appId;
            }

            appId = context.Request.Query["app_id"].ToString();
            return (!string.IsNullOrEmpty(appId)
                ? appId.ToString()
                : context.User.FindFirst("AppId")?.Value) ?? null;
        }
    }

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