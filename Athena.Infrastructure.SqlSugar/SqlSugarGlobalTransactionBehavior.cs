using System.Data;
using System.Diagnostics;
using Athena.Infrastructure.EventStorage;
using Athena.Infrastructure.EventStorage.Events;
using Athena.Infrastructure.EventStorage.Models;
using Athena.Infrastructure.Helpers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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
    private readonly IPublisher _publisher;
    private readonly ILogger<SqlSugarGlobalTransactionBehavior<TRequest, TResponse>> _logger;
    private readonly ICapPublisher? _capPublisher;
    private readonly ISecurityContextAccessor? _securityContextAccessor;
    private readonly IOptionsMonitor<EventStorageOptions>? _eventStorageOptions;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="publisher"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="sqlSugarClient"></param>
    /// <param name="domainEventContext"></param>
    /// <param name="integrationEventContext"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="securityContextAccessor"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public SqlSugarGlobalTransactionBehavior(
        IPublisher publisher,
        ILoggerFactory loggerFactory,
        ISqlSugarClient sqlSugarClient,
        IDomainEventContext domainEventContext,
        IIntegrationEventContext integrationEventContext,
        IServiceProvider serviceProvider, ISecurityContextAccessor? securityContextAccessor)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _logger = loggerFactory.CreateLogger<SqlSugarGlobalTransactionBehavior<TRequest, TResponse>>();
        _sqlSugarClient = sqlSugarClient;
        _domainEventContext = domainEventContext;
        _integrationEventContext = integrationEventContext;
        _securityContextAccessor = securityContextAccessor;
        _capPublisher = serviceProvider.GetService<ICapPublisher>();
        _eventStorageOptions = AthenaProvider.GetService<IOptionsMonitor<EventStorageOptions>>();
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
                if (_sqlSugarClient.AsTenant().IsAnyConnection(TenantId))
                {
                    var scope = _sqlSugarClient.AsTenant().GetConnectionScope(TenantId);
                    scope.CurrentConnectionConfig.IsAutoCloseConnection = false;
                    dbConnection = scope.Ado.Connection;
                }
                else
                {
                    _sqlSugarClient.CurrentConnectionConfig.IsAutoCloseConnection = false;
                    dbConnection = _sqlSugarClient.Ado.Connection;
                }
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
            await DomainEventHandleAsync(rootTraceId, cancellationToken);
            // 集成事件发布处理
            await IntegrationEventHandleAsync(rootTraceId, cancellationToken);
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
        catch (VersionExceptions ex)
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

            throw FriendlyException.Of("数据已被修改，请刷新后重试");
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

                if (@event is {IsDelayMessage: true, DelayTime: not null})
                {
                    await _capPublisher.PublishDelayAsync(
                        @event.DelayTime.Value,
                        @event.EventName,
                        @event,
                        @event.CallbackName,
                        cancellationToken
                    );
                    continue;
                }

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