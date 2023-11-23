namespace Athena.Infrastructure.FreeSql.Bases;

/// <summary>
/// 服务基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class ServiceBase<T> : QueryServiceBase<T> where T : EntityCore, new()
{
    private const string DomainEventKey = "DomainEvent";
    private const string IntegrationEventKey = "IntegrationEvent";
    private ConcurrentDictionary<string, object?>? _repositories;
    private UnitOfWorkManager? _unitOfWorkManager;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unitOfWorkManager"></param>
    public ServiceBase(UnitOfWorkManager unitOfWorkManager) : base(unitOfWorkManager.Orm)
    {
        _unitOfWorkManager = unitOfWorkManager;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unitOfWorkManager"></param>
    /// <param name="accessor"></param>
    public ServiceBase(UnitOfWorkManager unitOfWorkManager, ISecurityContextAccessor accessor) :
        base(unitOfWorkManager.Orm, accessor)
    {
        _unitOfWorkManager = unitOfWorkManager;
    }

    /// <summary>
    /// 设置
    /// </summary>
    /// <param name="unitOfWorkManager"></param>
    protected void SetUnitOfWorkManager(UnitOfWorkManager unitOfWorkManager)
    {
        _unitOfWorkManager = unitOfWorkManager;
        SetContext(_unitOfWorkManager.Orm);
    }

    /// <summary>
    /// 读取信息
    /// </summary>
    /// <param name="id">ID</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="FriendlyException"></exception>
    protected Task<T> GetAsync(string? id, CancellationToken cancellationToken = default)
    {
        return GetAsync<T>(id, "数据", cancellationToken);
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
    protected Task<T> GetAsync(string? id, string name,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<T>(id, name, cancellationToken);
    }

    /// <summary>
    /// 读取信息
    /// </summary>
    /// <param name="id">ID</param>
    /// <param name="name">名称</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="FriendlyException"></exception>
    protected Task<TEntity> GetAsync<TEntity>(string? id, string name,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        if (string.IsNullOrEmpty(id))
        {
            throw FriendlyException.NullOrEmptyArgument("id");
        }

        var entity = Query<TEntity>()
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
    [Obsolete("请使用GetAsync方法")]
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
    [Obsolete("请使用GetAsync方法")]
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
    protected override ISelect<T> Queryable => GetDefaultRepository<T, string>().Select;

    /// <summary>
    /// 查询对象
    /// </summary>
    protected override ISelect<T> QueryableWithSoftDelete =>
        GetDefaultRepository<T, string>().Select.DisableGlobalFilter(SoftDelete);

    /// <summary>
    /// 查询对象
    /// </summary>
    /// <returns></returns>
    protected override ISelect<T> Query()
    {
        return Queryable;
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    /// <returns></returns>
    protected override ISelect<T> QueryWithSoftDelete()
    {
        return QueryableWithSoftDelete;
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    protected override ISelect<T1> Query<T1>()
    {
        // 如果T1是ValueObject,，则使用long
        return typeof(ValueObject).IsAssignableFrom(typeof(T1))
            ? GetOtherRepository<T1, long>().Select
            : GetOtherRepository<T1, string>().Select;
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    protected override ISelect<T1> QueryWithSoftDelete<T1>()
    {
        return Query<T1>().DisableGlobalFilter(SoftDelete);
    }

    #endregion


    #region 工作单元

    private DefaultRepository<TEntity, TEntityKey> GetOtherRepository<TEntity, TEntityKey>()
        where TEntity : class
    {
        if (_unitOfWorkManager == null)
        {
            throw new ArgumentNullException(nameof(UnitOfWorkManager), "未注入[UnitOfWorkManager]实例");
        }

        _repositories ??= new ConcurrentDictionary<string, object?>();
        var entityKey = typeof(TEntity).Name;
        if (_repositories.GetOrAdd(entityKey,
                new DefaultRepository<TEntity, TEntityKey>(FreeSqlDbContext, _unitOfWorkManager)) is not
            DefaultRepository<TEntity, TEntityKey> repository)
        {
            throw new ArgumentNullException(nameof(repository), "获取[DefaultRepository]失败");
        }

        if (!typeof(ITenant).IsAssignableFrom(typeof(TEntity)))
        {
            repository.DataFilter.Disable(DefaultTenant, OtherTenant);
        }

        return repository;
    }

    private DefaultRepository<TEntity, TEntityKey> GetDefaultRepository<TEntity, TEntityKey>()
        where TEntity : EntityCore, new()
    {
        if (_unitOfWorkManager == null)
        {
            throw new ArgumentNullException(nameof(UnitOfWorkManager), "未注入[UnitOfWorkManager]实例");
        }

        _repositories ??= new ConcurrentDictionary<string, object?>();

        var entityKey = typeof(TEntity).Name;
        if (_repositories.GetOrAdd(entityKey,
                new DefaultRepository<TEntity, TEntityKey>(FreeSqlDbContext, _unitOfWorkManager)) is not
            DefaultRepository<TEntity, TEntityKey> repository)
        {
            throw new ArgumentNullException(nameof(repository), "获取[DefaultRepository]失败");
        }

        if (!typeof(ITenant).IsAssignableFrom(typeof(TEntity)))
        {
            repository.DataFilter.Disable(DefaultTenant, OtherTenant);
        }

        return repository;
    }

    #region 值对象

    private DefaultRepository<TValueObject, long> GetDefaultValueObjectRepository<TValueObject>()
        where TValueObject : ValueObject, new()
    {
        if (_unitOfWorkManager == null)
        {
            throw new ArgumentNullException(nameof(UnitOfWorkManager), "未注入[UnitOfWorkManager]实例");
        }

        _repositories ??= new ConcurrentDictionary<string, object?>();

        if (_repositories.GetOrAdd(typeof(TValueObject).Name,
                new DefaultRepository<TValueObject, long>(FreeSqlDbContext, _unitOfWorkManager)) is not
            DefaultRepository<TValueObject, long> repository)
        {
            throw new ArgumentNullException(nameof(repository), "获取[DefaultRepository]失败");
        }

        repository.DataFilter.Disable(DefaultTenant, OtherTenant);
        return repository;
    }


    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TValueObject"></typeparam>
    protected TValueObject RegisterNewValueObject<TValueObject>(TValueObject entity)
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
    protected Task<TValueObject> RegisterNewValueObjectAsync<TValueObject>(TValueObject entity,
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
    protected List<TValueObject> RegisterNewRangeValueObject<TValueObject>(List<TValueObject> entities)
        where TValueObject : ValueObject, new()
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentNullException(nameof(entities));
        }

        return GetDefaultValueObjectRepository<TValueObject>().Insert(entities);
    }

    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TValueObject"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected Task<List<TValueObject>> RegisterNewRangeValueObjectAsync<TValueObject>(List<TValueObject> entities,
        CancellationToken cancellationToken = default) where TValueObject : ValueObject, new()
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentNullException(nameof(entities));
        }

        return GetDefaultValueObjectRepository<TValueObject>().InsertAsync(entities, cancellationToken);
    }


    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <typeparam name="TValueObject"></typeparam>
    protected int RegisterDeleteValueObject<TValueObject>(Expression<Func<TValueObject, bool>> exp)
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
    protected Task<int> RegisterDeleteValueObjectAsync<TValueObject>(Expression<Func<TValueObject, bool>> exp,
        CancellationToken cancellationToken = default)
        where TValueObject : ValueObject, new()
    {
        return GetDefaultValueObjectRepository<TValueObject>().DeleteAsync(exp, cancellationToken);
    }

    #endregion

    #region 聚合根

    #region 新增

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity"></param>
    protected virtual T RegisterNew(T entity)
    {
        return RegisterNew<string>(entity);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected TEntity RegisterNew<TEntity>(TEntity entity)
        where TEntity : EntityCore, new()
    {
        return RegisterNew<TEntity, string>(entity);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    protected virtual Task<T> RegisterNewAsync(T entity,
        CancellationToken cancellationToken = default)
    {
        return RegisterNewAsync<string>(entity, cancellationToken);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected Task<TEntity> RegisterNewAsync<TEntity>(TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        return RegisterNewAsync<TEntity, string>(entity, cancellationToken);
    }

    /// <summary>
    /// 添加事件
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    private void TryAddEvent<TEntity>(TEntity entity) where TEntity : EntityCore, new()
    {
        #region 领域事件

        var domainEventKey = GetDomainEventKey(entity.Id);
        var domainEvents = _unitOfWorkManager
            ?.Current
            ?.States
            ?.FirstOrDefault(p => p.Key == domainEventKey)
            .Value as HashSet<IDomainEvent> ?? new HashSet<IDomainEvent>();
        if (domainEvents is {Count: > 0} || entity.DomainEvents.Count > 0)
        {
            foreach (var domainEvent in domainEvents)
            {
                entity.DomainEvents.Add(domainEvent);
            }
        }

        // 先移除再新增
        _unitOfWorkManager?.Current?.States?.Remove(domainEventKey, out _);
        _unitOfWorkManager?.Current?.States?.TryAdd(domainEventKey, entity.DomainEvents);

        #endregion

        #region 集成事件

        var integrationEventKey = GetIntegrationEventKey(entity.Id);
        var integrationEvents = _unitOfWorkManager
            ?.Current
            ?.States
            ?.FirstOrDefault(p => p.Key == integrationEventKey)
            .Value as HashSet<IIntegrationEvent> ?? new HashSet<IIntegrationEvent>();
        if (integrationEvents is {Count: > 0} || entity.IntegrationEvents.Count > 0)
        {
            foreach (var integrationEvent in integrationEvents)
            {
                entity.IntegrationEvents.Add(integrationEvent);
            }
        }

        // 先移除再新增
        _unitOfWorkManager?.Current?.States?.Remove(integrationEventKey, out _);
        _unitOfWorkManager?.Current?.States?.TryAdd(integrationEventKey, entity.IntegrationEvents);

        #endregion
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntityKey"></typeparam>
    protected T RegisterNew<TEntityKey>(T entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        SetOtherValues(entity);
        TryAddEvent(entity);

        return GetDefaultRepository<T, TEntityKey>().Insert(entity);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected TEntity RegisterNew<TEntity, TEntityKey>(TEntity entity)
        where TEntity : EntityCore, new()
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        SetOtherValues(entity);
        TryAddEvent(entity);

        return GetDefaultRepository<TEntity, TEntityKey>().Insert(entity);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntityKey"></typeparam>
    protected Task<T> RegisterNewAsync<TEntityKey>(T entity,
        CancellationToken cancellationToken = default)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        SetOtherValues(entity);
        TryAddEvent(entity);

        return GetDefaultRepository<T, TEntityKey>().InsertAsync(entity, cancellationToken);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected Task<TEntity> RegisterNewAsync<TEntity, TEntityKey>(TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        SetOtherValues(entity);
        TryAddEvent(entity);

        return GetDefaultRepository<TEntity, TEntityKey>().InsertAsync(entity, cancellationToken);
    }


    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual List<T> RegisterNewRange(List<T> entities)
    {
        return RegisterNewRange<string>(entities);
    }

    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected List<TEntity> RegisterNewRange<TEntity>(List<TEntity> entities) where TEntity : EntityCore, new()
    {
        return RegisterNewRange<TEntity, string>(entities);
    }

    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual Task<List<T>> RegisterNewRangeAsync(List<T> entities,
        CancellationToken cancellationToken)
    {
        return RegisterNewRangeAsync<string>(entities, cancellationToken);
    }

    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected Task<List<TEntity>> RegisterNewRangeAsync<TEntity>(List<TEntity> entities,
        CancellationToken cancellationToken)
        where TEntity : EntityCore, new()
    {
        return RegisterNewRangeAsync<TEntity, string>(entities, cancellationToken);
    }


    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <typeparam name="TEntityKey"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected List<T> RegisterNewRange<TEntityKey>(List<T> entities)
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

        return GetDefaultRepository<T, TEntityKey>().Insert(entities);
    }

    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected List<TEntity> RegisterNewRange<TEntity, TEntityKey>(List<TEntity> entities)
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

        return GetDefaultRepository<TEntity, TEntityKey>().Insert(entities);
    }

    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntityKey"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected Task<List<T>> RegisterNewRangeAsync<TEntityKey>(List<T> entities,
        CancellationToken cancellationToken = default)
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

        return GetDefaultRepository<T, TEntityKey>().InsertAsync(entities, cancellationToken);
    }

    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected Task<List<TEntity>> RegisterNewRangeAsync<TEntity, TEntityKey>(List<TEntity> entities,
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

        return GetDefaultRepository<TEntity, TEntityKey>().InsertAsync(entities, cancellationToken);
    }

    #endregion

    #region 修改

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected virtual int RegisterDirty<TEntity>(TEntity entity)
        where TEntity : EntityCore, new()
    {
        return RegisterDirty<TEntity, string>(entity);
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected virtual Task<int> RegisterDirtyAsync<TEntity>(TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        return RegisterDirtyAsync<TEntity, string>(entity, cancellationToken);
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected int RegisterDirty<TEntity, TEntityKey>(TEntity entity)
        where TEntity : EntityCore, new()
    {
        var hasSoftDelete = typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));
        var delProperty = typeof(TEntity).GetProperty(nameof(ISoftDelete.IsDeleted));
        var isDeleted = Convert.ToBoolean(delProperty?.GetValue(entity));
        // 如果不是逻辑删除，或者是逻辑删除但是未删除，则设置更新时间
        if (!hasSoftDelete || !isDeleted)
        {
            entity.UpdatedOn = DateTime.Now;
            // 更新人
            if (typeof(IUpdater).IsAssignableFrom(typeof(TEntity)))
            {
                var property = typeof(TEntity).GetProperty(nameof(IUpdater.LastUpdatedUserId));
                if (property?.GetValue(entity) == null)
                {
                    // 动态设置更新人
                    property?.SetValue(entity, UserId);
                }
            }
        }

        TryAddEvent(entity);
        return GetDefaultRepository<TEntity, TEntityKey>().Update(entity);
    }

    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected Task<int> RegisterDirtyAsync<TEntity, TEntityKey>(TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        var hasSoftDelete = typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));
        var delProperty = typeof(TEntity).GetProperty(nameof(ISoftDelete.IsDeleted));
        var isDeleted = Convert.ToBoolean(delProperty?.GetValue(entity));
        // 如果不是逻辑删除，或者是逻辑删除但是未删除，则设置更新时间
        if (!hasSoftDelete || !isDeleted)
        {
            entity.UpdatedOn = DateTime.Now;
            // 更新人
            if (typeof(IUpdater).IsAssignableFrom(typeof(TEntity)))
            {
                var property = typeof(TEntity).GetProperty(nameof(IUpdater.LastUpdatedUserId));
                if (property?.GetValue(entity) == null)
                {
                    // 动态设置更新人
                    property?.SetValue(entity, UserId);
                }
            }
        }

        TryAddEvent(entity);
        return GetDefaultRepository<TEntity, TEntityKey>().UpdateAsync(entity, cancellationToken);
    }

    /// <summary>
    /// 批量修改
    /// </summary>
    /// <param name="entities"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual int RegisterDirtyRange<TEntity>(List<TEntity> entities) where TEntity : EntityCore, new()
    {
        return RegisterDirtyRange<TEntity, string>(entities);
    }

    /// <summary>
    /// 批量修改
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual Task<int> RegisterDirtyRangeAsync<TEntity>(List<TEntity> entities,
        CancellationToken cancellationToken = default) where TEntity : EntityCore, new()
    {
        return RegisterDirtyRangeAsync<TEntity, string>(entities, cancellationToken);
    }

    /// <summary>
    /// 批量修改
    /// </summary>
    /// <param name="entities"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected int RegisterDirtyRange<TEntity, TEntityKey>(List<TEntity> entities)
        where TEntity : EntityCore, new()
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentNullException(nameof(entities));
        }

        var hasUpdater = typeof(IUpdater).IsAssignableFrom(typeof(TEntity));
        var hasSoftDelete = typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));

        foreach (var entity in entities)
        {
            var delProperty = typeof(TEntity).GetProperty(nameof(ISoftDelete.IsDeleted));
            var isDeleted = Convert.ToBoolean(delProperty?.GetValue(entity));
            // 如果是逻辑删除，则跳过设置更新时间
            if (hasSoftDelete && isDeleted)
            {
                TryAddEvent(entity);
                continue;
            }

            entity.UpdatedOn = DateTime.Now;
            // 更新人
            if (hasUpdater)
            {
                var property = typeof(TEntity).GetProperty(nameof(IUpdater.LastUpdatedUserId));
                if (property?.GetValue(entity) == null)
                {
                    // 动态设置更新人
                    property?.SetValue(entity, UserId);
                }
            }

            TryAddEvent(entity);
        }

        return GetDefaultRepository<TEntity, TEntityKey>().Update(entities);
    }

    /// <summary>
    /// 批量修改
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    protected Task<int> RegisterDirtyRangeAsync<TEntity, TEntityKey>(List<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentNullException(nameof(entities));
        }

        var hasUpdater = typeof(IUpdater).IsAssignableFrom(typeof(TEntity));
        var hasSoftDelete = typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));
        foreach (var entity in entities)
        {
            var delProperty = typeof(TEntity).GetProperty(nameof(ISoftDelete.IsDeleted));
            var isDeleted = Convert.ToBoolean(delProperty?.GetValue(entity));
            // 如果是逻辑删除，则跳过设置更新时间
            if (hasSoftDelete && isDeleted)
            {
                TryAddEvent(entity);
                continue;
            }

            entity.UpdatedOn = DateTime.Now;
            // 更新人
            if (hasUpdater)
            {
                var property = typeof(TEntity).GetProperty(nameof(IUpdater.LastUpdatedUserId));
                if (property?.GetValue(entity) == null)
                {
                    // 动态设置更新人
                    property?.SetValue(entity, UserId);
                }
            }

            TryAddEvent(entity);
        }

        return GetDefaultRepository<TEntity, TEntityKey>().UpdateAsync(entities, cancellationToken);
    }

    #endregion

    #region 物理删除

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected int RegisterDelete<TEntity>(TEntity entity)
        where TEntity : EntityCore, new()
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        TryAddEvent(entity);

        return GetDefaultRepository<TEntity, string>().Delete(entity);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected Task<int> RegisterDeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        TryAddEvent(entity);

        return GetDefaultRepository<TEntity, string>().DeleteAsync(entity, cancellationToken);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected int RegisterDelete<TEntity, TEntityKey>(TEntity entity)
        where TEntity : EntityCore, new()
    {
        return RegisterDeleteRange<TEntity, TEntityKey>(new List<TEntity> {entity});
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected Task RegisterDeleteAsync<TEntity, TEntityKey>(TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        return RegisterDeleteRangeAsync<TEntity, TEntityKey>(new List<TEntity> {entity}, cancellationToken);
    }


    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="entities"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected void RegisterDeleteRange<TEntity>(IList<TEntity> entities)
        where TEntity : EntityCore, new()
    {
        RegisterDeleteRange<TEntity, string>(entities);
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected Task<int> RegisterDeleteRangeAsync<TEntity>(IList<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        return RegisterDeleteRangeAsync<TEntity, string>(entities, cancellationToken);
    }


    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="entities"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected int RegisterDeleteRange<TEntity, TEntityKey>(IList<TEntity> entities)
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

        return GetDefaultRepository<TEntity, TEntityKey>().Delete(entities);
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected Task<int> RegisterDeleteRangeAsync<TEntity, TEntityKey>(IList<TEntity> entities,
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

        return GetDefaultRepository<TEntity, TEntityKey>().DeleteAsync(entities, cancellationToken);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    protected void RegisterDelete(Expression<Func<T, bool>> exp)
    {
        RegisterDelete<T, string>(exp);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <param name="cancellationToken"></param>
    protected Task<int> RegisterDeleteAsync(Expression<Func<T, bool>> exp,
        CancellationToken cancellationToken = default)
    {
        return RegisterDeleteAsync<T, string>(exp, cancellationToken);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected int RegisterDelete<TEntity>(Expression<Func<TEntity, bool>> exp)
        where TEntity : EntityCore, new()
    {
        return RegisterDelete<TEntity, string>(exp);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected Task<int> RegisterDeleteAsync<TEntity>(Expression<Func<TEntity, bool>> exp,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        return RegisterDeleteAsync<TEntity, string>(exp, cancellationToken);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected int RegisterDelete<TEntity, TEntityKey>(Expression<Func<TEntity, bool>> exp)
        where TEntity : EntityCore, new()
    {
        return GetDefaultRepository<TEntity, TEntityKey>().Delete(exp);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected Task<int> RegisterDeleteAsync<TEntity, TEntityKey>(Expression<Func<TEntity, bool>> exp,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, new()
    {
        return GetDefaultRepository<TEntity, TEntityKey>().DeleteAsync(exp, cancellationToken);
    }

    #endregion

    #region 逻辑删除

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected int RegisterSoftDelete<TEntity>(TEntity entity)
        where TEntity : EntityCore, ISoftDelete, new()
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        entity.IsDeleted = true;
        entity.DeletedOn = DateTime.Now;
        entity.DeletedUserId = UserId;

        return RegisterDirty(entity);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected Task<int> RegisterSoftDeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : EntityCore, ISoftDelete, new()
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        entity.IsDeleted = true;
        entity.DeletedOn = DateTime.Now;
        entity.DeletedUserId = UserId;

        return RegisterDirtyAsync(entity, cancellationToken);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected int RegisterSoftDelete<TEntity, TEntityKey>(TEntity entity)
        where TEntity : EntityCore, ISoftDelete, new()
    {
        return RegisterSoftDeleteRange<TEntity, TEntityKey>(new List<TEntity> {entity});
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected Task RegisterSoftDeleteAsync<TEntity, TEntityKey>(TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, ISoftDelete, new()
    {
        return RegisterSoftDeleteRangeAsync<TEntity, TEntityKey>(new List<TEntity> {entity}, cancellationToken);
    }


    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="entities"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected void RegisterSoftDeleteRange<TEntity>(List<TEntity> entities)
        where TEntity : EntityCore, ISoftDelete, new()
    {
        RegisterSoftDeleteRange<TEntity, string>(entities);
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected Task<int> RegisterSoftDeleteRangeAsync<TEntity>(List<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, ISoftDelete, new()
    {
        return RegisterSoftDeleteRangeAsync<TEntity, string>(entities, cancellationToken);
    }


    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="entities"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected int RegisterSoftDeleteRange<TEntity, TEntityKey>(List<TEntity> entities)
        where TEntity : EntityCore, ISoftDelete, new()
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentNullException(nameof(entities));
        }

        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
            entity.DeletedOn = DateTime.Now;
            entity.DeletedUserId = UserId;
        }

        return RegisterDirtyRange<TEntity, TEntityKey>(entities);
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected Task<int> RegisterSoftDeleteRangeAsync<TEntity, TEntityKey>(List<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, ISoftDelete, new()
    {
        if (entities == null || !entities.Any())
        {
            throw new ArgumentNullException(nameof(entities));
        }

        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
            entity.DeletedOn = DateTime.Now;
            entity.DeletedUserId = UserId;
        }

        return RegisterDirtyRangeAsync<TEntity, TEntityKey>(entities, cancellationToken);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    protected int RegisterSoftDelete(Expression<Func<T, bool>> exp)
    {
        if (!typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
        {
            return 0;
        }

        var entities = Query<T>()
            .Where(exp)
            .ToList();

        if (entities == null || !entities.Any())
        {
            return 0;
        }

        foreach (var entity in entities)
        {
            typeof(T).GetProperty(nameof(ISoftDelete.IsDeleted))?.SetValue(entity, true);
            typeof(T).GetProperty(nameof(ISoftDelete.DeletedOn))?.SetValue(entity, DateTime.Now);
            typeof(T).GetProperty(nameof(ISoftDelete.DeletedUserId))?.SetValue(entity, UserId);
        }

        return RegisterDirtyRange<T, string>(entities);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <param name="cancellationToken"></param>
    protected async Task<int> RegisterSoftDeleteAsync(Expression<Func<T, bool>> exp,
        CancellationToken cancellationToken = default)
    {
        if (!typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
        {
            return 0;
        }

        var entities = await Query<T>()
            .Where(exp)
            .ToListAsync(cancellationToken);

        if (entities == null || !entities.Any())
        {
            return 0;
        }

        foreach (var entity in entities)
        {
            typeof(T).GetProperty(nameof(ISoftDelete.IsDeleted))?.SetValue(entity, true);
            typeof(T).GetProperty(nameof(ISoftDelete.DeletedOn))?.SetValue(entity, DateTime.Now);
            typeof(T).GetProperty(nameof(ISoftDelete.DeletedUserId))?.SetValue(entity, UserId);
        }

        return await RegisterDirtyRangeAsync<T, string>(entities, cancellationToken);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected int RegisterSoftDelete<TEntity>(Expression<Func<TEntity, bool>> exp)
        where TEntity : EntityCore, ISoftDelete, new()
    {
        return RegisterSoftDelete<TEntity, string>(exp);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    protected Task<int> RegisterSoftDeleteAsync<TEntity>(Expression<Func<TEntity, bool>> exp,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, ISoftDelete, new()
    {
        return RegisterSoftDeleteAsync<TEntity, string>(exp, cancellationToken);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected int RegisterSoftDelete<TEntity, TEntityKey>(Expression<Func<TEntity, bool>> exp)
        where TEntity : EntityCore, ISoftDelete, new()
    {
        var entities = Query<TEntity>()
            .Where(exp)
            .ToList();
        return entities.Count == 0 ? 0 : RegisterSoftDeleteRange<TEntity, TEntityKey>(entities);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="exp"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityKey"></typeparam>
    protected async Task<int> RegisterSoftDeleteAsync<TEntity, TEntityKey>(Expression<Func<TEntity, bool>> exp,
        CancellationToken cancellationToken = default)
        where TEntity : EntityCore, ISoftDelete, new()
    {
        var entities = await Query<TEntity>()
            .Where(exp)
            .ToListAsync(cancellationToken);
        return entities.Count == 0
            ? 0
            : await RegisterSoftDeleteRangeAsync<TEntity, TEntityKey>(entities, cancellationToken);
    }

    #endregion

    #endregion

    #endregion

    /// <summary>
    /// Repository
    /// </summary>
    protected IBaseRepository<T, string> Repository => FreeSqlDbContext.GetRepository<T, string>();

    /// <summary>
    /// Repository
    /// </summary>
    protected IBaseRepository<T1, string> GetRepository<T1>() where T1 : class
    {
        return FreeSqlDbContext.GetRepository<T1, string>();
    }

    /// <summary>
    /// Repository
    /// </summary>
    protected IBaseRepository<T1, TKey> GetRepository<T1, TKey>() where T1 : class
    {
        return FreeSqlDbContext.GetRepository<T1, TKey>();
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
            var property = typeof(TEntity).GetProperty(nameof(ITenant.TenantId));
            if (property?.GetValue(entity) == null)
            {
                // 动态设置租户ID
                property?.SetValue(entity, TenantId);
            }
        }

        // 创建人
        if (typeof(ICreator).IsAssignableFrom(typeof(TEntity)))
        {
            var property = typeof(TEntity).GetProperty(nameof(ICreator.CreatedUserId));
            if (property?.GetValue(entity) == null)
            {
                // 动态设置创建人
                property?.SetValue(entity, UserId);
            }
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
                GetDefaultValueObjectRepository<OrganizationalUnitAuth>().InsertAsync(orgIds);
            }

            var property = typeof(TEntity).GetProperty(nameof(IOrganization.OrganizationalUnitId));
            if (property?.GetValue(entity) == null)
            {
                // 动态设置组织架构
                typeof(TEntity).GetProperty(nameof(IOrganization.OrganizationalUnitId))
                    ?.SetValue(entity, orgIds?.FirstOrDefault()?.OrganizationalUnitId);
            }
        }
    }
}