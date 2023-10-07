namespace Athena.Infrastructure.FreeSql.EventContexts;

/// <summary>
/// 集成事件
/// </summary>
public class IntegrationEventContext : IIntegrationEventContext
{
    private readonly UnitOfWorkManager _unitOfWorkManager;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unitOfWorkManager"></param>
    public IntegrationEventContext(UnitOfWorkManager unitOfWorkManager)
    {
        _unitOfWorkManager = unitOfWorkManager;
    }

    /// <summary>
    /// 读取事件
    /// </summary>
    /// <returns></returns>
    public IList<IIntegrationEvent> GetEvents()
    {
        var uow = _unitOfWorkManager.Current;

        var list = new List<IIntegrationEvent>();
        foreach (var state in uow.States)
        {
            if (!state.Key.Contains("IntegrationEvent") || state.Value is not HashSet<IIntegrationEvent> stateValues)
            {
                continue;
            }

            foreach (var c in stateValues)
            {
                //IMPORTANT: because we have identity
                c.MetaData.TryAdd("id", state.Key.Split(",")[1]);
                c.MetaData.TryAdd("type", 2);
            }

            list.AddRange(stateValues);
            // 移除
            _unitOfWorkManager.Current.States.Remove(state.Key);
        }

        return list;
    }
}