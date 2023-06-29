using System.Data;
using Athena.Infrastructure.Event.IntegrationEvents;
using Microsoft.AspNetCore.Http;

namespace Athena.Infrastructure.SqlSugar;

/// <summary>
/// SqlSugarGlobalTransactionBehavior
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class SqlSugarGlobalTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly IDomainEventContext _domainEventContext;
    private readonly IIntegrationEventContext _integrationEventContext;
    private readonly IMediator _mediator;
    private readonly ILogger<SqlSugarGlobalTransactionBehavior<TRequest, TResponse>> _logger;
    private readonly ICapPublisher? _capPublisher;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="sqlSugarClient"></param>
    /// <param name="domainEventContext"></param>
    /// <param name="integrationEventContext"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="httpContextAccessor"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public SqlSugarGlobalTransactionBehavior(
        IMediator mediator,
        ILoggerFactory loggerFactory,
        ISqlSugarClient sqlSugarClient,
        IDomainEventContext domainEventContext,
        IIntegrationEventContext integrationEventContext,
        IServiceProvider serviceProvider, IHttpContextAccessor? httpContextAccessor)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = loggerFactory.CreateLogger<SqlSugarGlobalTransactionBehavior<TRequest, TResponse>>();
        _sqlSugarClient = sqlSugarClient;
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

        IDbConnection? dbConnection = null;
        IDbTransaction? dbTransaction = null;
        ICapTransaction? capTransaction = null;
        try
        {
            if (string.IsNullOrEmpty(TenantId))
            {
                _sqlSugarClient.CurrentConnectionConfig.IsAutoCloseConnection = false;
                dbConnection = _sqlSugarClient.Ado.Connection;
            }
            else
            {
                var scope = _sqlSugarClient.AsTenant().GetConnectionScope(TenantId);
                scope.CurrentConnectionConfig.IsAutoCloseConnection = false;
                dbConnection = scope.Ado.Connection;
            }

            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            if (_capPublisher != null)
            {
                capTransaction = _capPublisher.BeginTransaction(dbConnection, autoCommit: false);
                _sqlSugarClient.Ado.Transaction = (IDbTransaction) capTransaction.DbTransaction!;
                dbTransaction = (IDbTransaction) capTransaction.DbTransaction!;
            }
            else
            {
                dbTransaction = dbConnection.BeginTransaction();
            }

            // 执行方法
            var response = await next();
            // 领域事件发布处理
            await DomainEventHandleAsync(cancellationToken);
            // 集成事件发布处理
            await IntegrationEventHandleAsync(cancellationToken);
            // 提交事务
            Commit(capTransaction, dbTransaction);
            return response;
        }
        catch (FriendlyException ex)
        {
            _logger.LogWarning(ex, "{Message}", ex.Message);
            if (capTransaction != null)
            {
                await capTransaction.RollbackAsync(cancellationToken);
            }
            else
            {
                dbTransaction?.Rollback();
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            if (capTransaction != null)
            {
                await capTransaction.RollbackAsync(cancellationToken);
            }
            else
            {
                dbTransaction?.Rollback();
            }

            throw;
        }
        finally
        {
            if (capTransaction != null)
            {
                capTransaction.Dispose();
            }
            else
            {
                dbTransaction?.Dispose();
            }

            if (dbConnection?.State != ConnectionState.Closed)
            {
                dbConnection?.Close();
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
    /// <param name="dbTransaction"></param>
    private static void Commit(ICapTransaction? capTransaction, IDbTransaction? dbTransaction)
    {
        if (capTransaction == null)
        {
            dbTransaction?.Commit();
        }
        else
        {
            capTransaction.Commit();
        }
    }
}