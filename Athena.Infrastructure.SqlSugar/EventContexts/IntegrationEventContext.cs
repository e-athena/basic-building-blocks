using Athena.Infrastructure.Event.IntegrationEvents;

namespace Athena.Infrastructure.SqlSugar.EventContexts;

/// <summary>
/// 集成事件
/// </summary>
public class IntegrationEventContext : IIntegrationEventContext
{
    private readonly ISqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    public IntegrationEventContext(ISqlSugarClient sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
    }


    /// <summary>
    /// 读取事件
    /// </summary>
    /// <returns></returns>
    public IList<IIntegrationEvent> GetEvents()
    {
        var list = new List<IIntegrationEvent>();
        foreach (var state in _sqlSugarClient.TempItems)
        {
            if (!state.Key.Contains("IntegrationEvent") || state.Value is not HashSet<IIntegrationEvent> stateValues)
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
            _sqlSugarClient.TempItems.Remove(state.Key);
        }

        return list;
    }
}