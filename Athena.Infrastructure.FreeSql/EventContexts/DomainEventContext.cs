namespace Athena.Infrastructure.FreeSql.EventContexts;

/// <summary>
/// 领域事件
/// </summary>
public class DomainEventContext : IDomainEventContext
{
    private readonly UnitOfWorkManager _unitOfWorkManager;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unitOfWorkManager"></param>
    public DomainEventContext(UnitOfWorkManager unitOfWorkManager)
    {
        _unitOfWorkManager = unitOfWorkManager;
    }

    /// <summary>
    /// 读取事件
    /// </summary>
    /// <returns></returns>
    public IList<IDomainEvent> GetEvents()
    {
        var uow = _unitOfWorkManager.Current;

        var list = new List<IDomainEvent>();
        foreach (var state in uow.States)
        {
            if (!state.Key.Contains("DomainEvent") || state.Value is not HashSet<IDomainEvent> stateValues)
            {
                continue;
            }

            foreach (var c in stateValues)
            {
                //IMPORTANT: because we have identity
                c.MetaData.TryAdd("id", state.Key.Split(",")[1]);
            }

            list.AddRange(stateValues);
            // 移除
            _unitOfWorkManager.Current.States.Remove(state.Key);
        }

        return list;
    }
}