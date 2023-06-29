namespace Athena.Infrastructure.SqlSugar.EventContexts;

/// <summary>
/// 领域事件
/// </summary>
public class DomainEventContext : IDomainEventContext
{
    private readonly ISqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    public DomainEventContext(ISqlSugarClient sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 读取事件
    /// </summary>
    /// <returns></returns>
    public IList<IDomainEvent> GetEvents()
    {
        var list = new List<IDomainEvent>();
        foreach (var state in _sqlSugarClient.TempItems)
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
            _sqlSugarClient.TempItems.Remove(state.Key);
        }

        return list;
    }
}