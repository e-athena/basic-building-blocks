using ITenant = Athena.Infrastructure.Domain.ITenant;

namespace Athena.Infrastructure.SqlSugar.Bases;

/// <summary>
/// 服务基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class ServiceBase<T> : QueryServiceBase<T> where T : EntityCore, new()
{
    private const string DomainEventKey = "DomainEvent";
    private const string IntegrationEventKey = "IntegrationEvent";
    private ConcurrentDictionary<string, object?>? _repositories;
    private readonly ISqlSugarClient? _sqlSugarClient;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    public ServiceBase(ISqlSugarClient sqlSugarClient) : base(sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    /// <param name="accessor"></param>
    public ServiceBase(ISqlSugarClient sqlSugarClient, ISecurityContextAccessor accessor) :
        base(sqlSugarClient, accessor)
    {
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 读取信息
    /// </summary>
    /// <param name="id">ID</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="FriendlyException"></exception>
    protected Task<TEntity> GetAsync<TEntity>(string? id,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        return GetAsync<TEntity>(id, "数据", cancellationToken);
    }

    /// <summary>
    /// 读取信息
    /// </summary>
    /// <param name="id">ID</param>
    /// <param name="name">名称</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="FriendlyException"></exception>
    protected async Task<TEntity> GetAsync<TEntity>(string? id, string name,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        if (string.IsNullOrEmpty(id))
        {
            throw FriendlyException.NullOrEmptyArgument("id");
        }

        var entity = await Query<TEntity>()
            .Where(p => p.Id == id)
            .FirstAsync(cancellationToken);

        if (entity == null)
        {
            throw FriendlyException.NotFound(name);
        }

        return entity;
    }

    /// <summary>
    /// 读取信息
    /// </summary>
    /// <param name="id">ID</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="FriendlyException"></exception>
    protected Task<T> GetForUpdateAsync(string? id, CancellationToken cancellationToken = default)
    {
        return GetForUpdateAsync(id, "数据", cancellationToken);
    }

    /// <summary>
    /// 读取信息
    /// </summary>
    /// <param name="id">ID</param>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="FriendlyException"></exception>
    protected async Task<T> GetForUpdateAsync(string? id, string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw FriendlyException.NullOrEmptyArgument("id");
        }

        var entity = await Queryable
            .Where(p => p.Id == id)
            .FirstAsync(cancellationToken);

        if (entity == null)
        {
            throw FriendlyException.NotFound(name);
        }

        return entity;
    }

    #region Query

    /// <summary>
    /// 查询对象
    /// </summary>
    protected override ISugarQueryable<T> Queryable => GetDefaultRepository<T>().AsQueryable();

    /// <summary>
    /// 查询对象
    /// </summary>
    /// <returns></returns>
    protected override ISugarQueryable<T> Query()
    {
        return Queryable;
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    protected override ISugarQueryable<T1> Query<T1>()
    {
        return GetOtherRepository<T1>().AsQueryable();
    }

    #endregion

    #region 工作单元

    private SimpleClient<TEntity> GetOtherRepository<TEntity>()
        where TEntity : class, new()
    {
        if (_sqlSugarClient == null)
        {
            throw new ArgumentNullException(nameof(ISqlSugarClient), "未注入[ISqlSugarClient]实例");
        }

        _repositories ??= new ConcurrentDictionary<string, object?>();
        if (_repositories.GetOrAdd(typeof(TEntity).Name, _sqlSugarClient.GetSimpleClient<TEntity>()) is not
            SimpleClient<TEntity> repository)
        {
            throw new ArgumentNullException(nameof(repository), "获取[DefaultRepository]失败");
        }

        if (!typeof(ITenant).IsAssignableFrom(typeof(TEntity)))
        {
            repository.AsSugarClient().QueryFilter.Clear<ITenant>();
        }

        return repository;
    }

    private SimpleClient<TEntity> GetDefaultRepository<TEntity>()
        where TEntity : EntityCore, new()
    {
        if (_sqlSugarClient == null)
        {
            throw new ArgumentNullException(nameof(ISqlSugarClient), "未注入[ISqlSugarClient]实例");
        }

        _repositories ??= new ConcurrentDictionary<string, object?>();
        if (_repositories.GetOrAdd(typeof(TEntity).Name, _sqlSugarClient.GetSimpleClient<TEntity>()) is not
            SimpleClient<TEntity> repository)
        {
            throw new ArgumentNullException(nameof(repository), "获取[DefaultRepository]失败");
        }

        if (!typeof(ITenant).IsAssignableFrom(typeof(TEntity)))
        {
            repository.AsSugarClient().QueryFilter.Clear<ITenant>();
        }

        return repository;
    }

    #region 值对象

    private SimpleClient<TValueObject> GetDefaultValueObjectRepository<TValueObject>()
        where TValueObject : ValueObject, new()
    {
        if (_sqlSugarClient == null)
        {
            throw new ArgumentNullException(nameof(ISqlSugarClient), "未注入[ISqlSugarClient]实例");
        }

        _repositories ??= new ConcurrentDictionary<string, object?>();

        if (_repositories.GetOrAdd(typeof(TValueObject).Name, _sqlSugarClient.GetSimpleClient<TValueObject>()) is not
            SimpleClient<TValueObject> repository)
        {
            throw new ArgumentNullException(nameof(repository), "获取[DefaultRepository]失败");
        }

        repository.AsSugarClient().QueryFilter.Clear<ITenant>();

        return repository;
    }


    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TValueObject"></typeparam>
    protected bool RegisterNewValueObject<TValueObject>(TValueObject entity)
        where TValueObject : ValueObject, new()
    {
        return GetDefaultValueObjectRepository<TValueObject>().Insert(entity);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TValueObject"></typeparam>
    protected Task<bool> RegisterNewValueObjectAsync<TValueObject>(TValueObject entity,
        CancellationToken cancellationToken = default)
        where TValueObject : ValueObject, new()
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        return GetDefaultValueObjectRepository<TValueObject>().InsertAsync(entity, cancellationToken);
    }

    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <typeparam name="TValueObject"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected bool RegisterNewRangeValueObject<TValueObject>(List<TValueObject> entities)
        where TValueObject : ValueObject, new()
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentNullException(nameof(entities));
        }

        return GetDefaultValueObjectRepository<TValueObject>().InsertRange(entities);
    }

    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TValueObject"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected Task<bool> RegisterNewRangeValueObjectAsync<TValueObject>(List<TValueObject> entities,
        CancellationToken cancellationToken = default) where TValueObject : ValueObject, new()
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentNullException(nameof(entities));
        }

        return GetDefaultValueObjectRepository<TValueObject>().InsertRangeAsync(entities, cancellationToken);
    }


    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <typeparam name="TValueObject"></typeparam>
    protected bool RegisterDeleteValueObject<TValueObject>(Expression<Func<TValueObject, bool>> exp)
        where TValueObject : ValueObject, new()
    {
        return GetDefaultValueObjectRepository<TValueObject>().Delete(exp);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TValueObject"></typeparam>
    protected Task<bool> RegisterDeleteValueObjectAsync<TValueObject>(Expression<Func<TValueObject, bool>> exp,
        CancellationToken cancellationToken = default)
        where TValueObject : ValueObject, new()
    {
        return GetDefaultValueObjectRepository<TValueObject>().DeleteAsync(exp, cancellationToken);
    }

    #endregion

    #region 聚合根

    #region 新增

    /// <summary>
    /// 添加事件
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    private void TryAddEvent<TEntity>(TEntity entity) where TEntity : EntityCore, new()
    {
        #region 领域事件

        var domainEventKey = GetDomainEventKey(entity.Id);
        var domainEvents = _sqlSugarClient?.TempItems.FirstOrDefault(p => p.Key == domainEventKey)
            .Value as HashSet<IDomainEvent> ?? new HashSet<IDomainEvent>();
        if (domainEvents is {Count: > 0} || entity.DomainEvents.Count > 0)
        {
            foreach (var domainEvent in domainEvents)
            {
                entity.DomainEvents.Add(domainEvent);
            }
        }

        // 先移除再新增
        _sqlSugarClient?.TempItems.Remove(domainEventKey, out _);
        _sqlSugarClient?.TempItems.TryAdd(domainEventKey, entity.DomainEvents);

        #endregion

        #region 集成事件

        var integrationEventKey = GetIntegrationEventKey(entity.Id);
        var integrationEvents = _sqlSugarClient?.TempItems.FirstOrDefault(p => p.Key == integrationEventKey)
            .Value as HashSet<IIntegrationEvent> ?? new HashSet<IIntegrationEvent>();
        if (integrationEvents is {Count: > 0} || entity.IntegrationEvents.Count > 0)
        {
            foreach (var integrationEvent in integrationEvents)
            {
                entity.IntegrationEvents.Add(integrationEvent);
            }
        }

        // 先移除再新增
        _sqlSugarClient?.TempItems.Remove(integrationEventKey, out _);
        _sqlSugarClient?.TempItems.TryAdd(integrationEventKey, entity.IntegrationEvents);

        #endregion
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity"></param>
    protected virtual bool RegisterNew(T entity)
    {
        return RegisterNew<T>(entity);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected virtual bool RegisterNew<TEntity>(TEntity entity)
        where TEntity : EntityCore, new()
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        SetOtherValues(entity);
        TryAddEvent(entity);

        return GetDefaultRepository<TEntity>().Insert(entity);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    protected virtual Task<bool> RegisterNewAsync(T entity,
        CancellationToken cancellationToken = default)
    {
        return RegisterNewAsync<T>(entity, cancellationToken);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected virtual Task<bool> RegisterNewAsync<TEntity>(TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        SetOtherValues(entity);
        TryAddEvent(entity);

        return GetDefaultRepository<TEntity>().InsertAsync(entity, cancellationToken);
    }

    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual bool RegisterNewRange(List<T> entities)
    {
        return RegisterNewRange<T>(entities);
    }

    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual bool RegisterNewRange<TEntity>(List<TEntity> entities)
        where TEntity : EntityCore, new()
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentNullException(nameof(entities));
        }

        foreach (var entity in entities)
        {
            SetOtherValues(entity);
            TryAddEvent(entity);
        }

        return GetDefaultRepository<TEntity>().InsertRange(entities);
    }

    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual Task<bool> RegisterNewRangeAsync(List<T> entities,
        CancellationToken cancellationToken = default)
    {
        return RegisterNewRangeAsync<T>(entities, cancellationToken);
    }

    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual Task<bool> RegisterNewRangeAsync<TEntity>(List<TEntity> entities,
        CancellationToken cancellationToken = default) where TEntity : EntityCore, new()
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentNullException(nameof(entities));
        }

        foreach (var entity in entities)
        {
            SetOtherValues(entity);
            TryAddEvent(entity);
        }

        return GetDefaultRepository<TEntity>().InsertRangeAsync(entities, cancellationToken);
    }

    #endregion

    #region 修改

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="entity"></param>
    protected virtual bool RegisterDirty(T entity)
    {
        return RegisterDirty<T>(entity);
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected virtual bool RegisterDirty<TEntity>(TEntity entity)
        where TEntity : EntityCore, new()
    {
        entity.UpdatedOn = DateTime.Now;
        TryAddEvent(entity);
        return GetDefaultRepository<TEntity>().Update(entity);
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    protected virtual Task<bool> RegisterDirtyAsync(T entity,
        CancellationToken cancellationToken = default)
    {
        return RegisterDirtyAsync<T>(entity, cancellationToken);
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected virtual Task<bool> RegisterDirtyAsync<TEntity>(TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        entity.UpdatedOn = DateTime.Now;
        TryAddEvent(entity);
        return GetDefaultRepository<TEntity>().UpdateAsync(entity, cancellationToken);
    }

    /// <summary>
    /// 批量修改
    /// </summary>
    /// <param name="entities"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual bool RegisterDirtyRange(List<T> entities)
    {
        return RegisterDirtyRange<T>(entities);
    }

    /// <summary>
    /// 批量修改
    /// </summary>
    /// <param name="entities"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual bool RegisterDirtyRange<TEntity>(List<TEntity> entities)
        where TEntity : EntityCore, new()
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentNullException(nameof(entities));
        }

        foreach (var entity in entities)
        {
            entity.UpdatedOn = DateTime.Now;
            TryAddEvent(entity);
        }

        return GetDefaultRepository<TEntity>().UpdateRange(entities);
    }

    /// <summary>
    /// 批量修改
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual Task<bool> RegisterDirtyRangeAsync(List<T> entities,
        CancellationToken cancellationToken = default)
    {
        return RegisterDirtyRangeAsync<T>(entities, cancellationToken);
    }

    /// <summary>
    /// 批量修改
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual Task<bool> RegisterDirtyRangeAsync<TEntity>(List<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentNullException(nameof(entities));
        }

        foreach (var entity in entities)
        {
            entity.UpdatedOn = DateTime.Now;
            TryAddEvent(entity);
        }

        return GetDefaultRepository<TEntity>().UpdateRangeAsync(entities, cancellationToken);
    }

    #endregion

    #region 删除

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected bool RegisterDelete<TEntity>(TEntity entity)
        where TEntity : EntityCore, new()
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        TryAddEvent(entity);

        return GetDefaultRepository<TEntity>().Delete(entity);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="entity"></param>
    protected bool RegisterDelete(T entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        TryAddEvent(entity);

        return RegisterDelete<T>(entity);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected Task<bool> RegisterDeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        TryAddEvent(entity);

        return GetDefaultRepository<TEntity>().DeleteAsync(entity, cancellationToken);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    protected Task<bool> RegisterDeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        return RegisterDeleteAsync<T>(entity, cancellationToken);
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="entities"></param>
    protected bool RegisterDeleteRange(IList<T> entities)
    {
        return RegisterDeleteRange<T>(entities);
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="entities"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected bool RegisterDeleteRange<TEntity>(IList<TEntity> entities)
        where TEntity : EntityCore, new()
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentNullException(nameof(entities));
        }

        foreach (var entity in entities)
        {
            TryAddEvent(entity);
        }

        return GetDefaultRepository<TEntity>()
            .DeleteByIds(entities.Select(entity => entity.Id).Cast<dynamic>().ToArray());
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    protected Task<bool> RegisterDeleteRangeAsync(IList<T> entities,
        CancellationToken cancellationToken = default)
    {
        return RegisterDeleteRangeAsync<T>(entities, cancellationToken);
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected Task<bool> RegisterDeleteRangeAsync<TEntity>(IList<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentNullException(nameof(entities));
        }

        foreach (var entity in entities)
        {
            TryAddEvent(entity);
        }

        return GetDefaultRepository<TEntity>()
            .DeleteByIdsAsync(entities.Select(entity => entity.Id).Cast<dynamic>().ToArray(), cancellationToken);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    protected void RegisterDelete(Expression<Func<T, bool>> exp)
    {
        RegisterDelete<T>(exp);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <param name="cancellationToken"></param>
    protected Task<bool> RegisterDeleteAsync(Expression<Func<T, bool>> exp,
        CancellationToken cancellationToken = default)
    {
        return RegisterDeleteAsync<T>(exp, cancellationToken);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected bool RegisterDelete<TEntity>(Expression<Func<TEntity, bool>> exp)
        where TEntity : EntityCore, new()
    {
        return GetDefaultRepository<TEntity>().Delete(exp);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected Task<bool> RegisterDeleteAsync<TEntity>(Expression<Func<TEntity, bool>> exp,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        return GetDefaultRepository<TEntity>().DeleteAsync(exp, cancellationToken);
    }

    #endregion

    #endregion

    #endregion

    /// <summary>
    /// Repository
    /// </summary>
    protected SimpleClient<T1> GetRepository<T1>() where T1 : class, new()
    {
        if (_sqlSugarClient == null)
        {
            throw new ArgumentNullException(nameof(ISqlSugarClient), "未注入[ISqlSugarClient]实例");
        }

        return _sqlSugarClient.GetSimpleClient<T1>();
    }

    /// <summary>
    /// 获取领域事件KEY
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private static string GetDomainEventKey(string id)
    {
        return $"{DomainEventKey},{id}";
    }

    /// <summary>
    /// 获取集成事件KEY
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private static string GetIntegrationEventKey(string id)
    {
        return $"{IntegrationEventKey},{id}";
    }

    /// <summary>
    /// 动态设置其他值
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    private void SetOtherValues<TEntity>(TEntity entity)
        where TEntity : EntityCore, new()
    {
        // 租户ID
        // 如果实体实现了ITenant接口并且当前环境为租户环境，则设置租户ID
        if (typeof(ITenant).IsAssignableFrom(typeof(TEntity)) && IsTenantEnvironment)
        {
            // 动态设置租户ID
            typeof(TEntity).GetProperty(nameof(ITenant.TenantId))?.SetValue(entity, TenantId);
        }

        // 创建人
        if (typeof(ICreator).IsAssignableFrom(typeof(TEntity)))
        {
            // 动态设置创建人
            typeof(TEntity).GetProperty(nameof(ICreator.CreatedUserId))?.SetValue(entity, UserId);
        }

        // 组织架构
        if (typeof(IOrganization).IsAssignableFrom(typeof(TEntity)) && !string.IsNullOrEmpty(OrganizationalUnitIds))
        {
            var orgIds = OrganizationalUnitIds?
                .Split(',')
                .Select(orgId => new OrganizationalUnitAuth(orgId, entity.Id, typeof(TEntity).Name))
                .ToList();

            if (orgIds is {Count: > 1})
            {
                // 如果当前用户有多个组织架构，则批量新增关联信息，用于数据行权限
                GetDefaultValueObjectRepository<OrganizationalUnitAuth>().InsertRange(orgIds);
            }

            // 动态设置组织架构
            typeof(TEntity).GetProperty(nameof(IOrganization.OrganizationalUnitIds))
                ?.SetValue(entity, orgIds?.FirstOrDefault()?.OrganizationalUnitId);
        }
    }
}