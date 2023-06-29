using System.Data;

namespace Athena.Infrastructure.FreeSql.Tenants;

/// <summary>
/// 
/// </summary>
public class UnitOfWorkManagerCloud
{
    private readonly Dictionary<string, UnitOfWorkManager> _mManagers = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cloud"></param>
    public UnitOfWorkManagerCloud(FreeSqlMultiTenancy cloud)
    {
        MultiTenancy = cloud;
    }

    /// <summary>
    /// 
    /// </summary>
    public FreeSqlMultiTenancy MultiTenancy { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public UnitOfWorkManager GetUnitOfWorkManager(string db)
    {
        if (_mManagers.TryGetValue(db, out var uow) == false)
        {
            _mManagers.Add(db, uow = new UnitOfWorkManager(MultiTenancy.Use(db)));
        }

        return uow;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
        foreach (var uow in _mManagers.Values)
        {
            uow.Dispose();
        }

        _mManagers.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="db"></param>
    /// <param name="propagation"></param>
    /// <param name="isolationLevel"></param>
    /// <returns></returns>
    public IUnitOfWork Begin(string db, Propagation propagation = Propagation.Required,
        IsolationLevel? isolationLevel = null)
    {
        return GetUnitOfWorkManager(db).Begin(propagation, isolationLevel);
    }
}