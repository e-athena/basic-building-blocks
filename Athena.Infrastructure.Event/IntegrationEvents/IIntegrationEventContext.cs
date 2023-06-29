namespace Athena.Infrastructure.Event.IntegrationEvents;

/// <summary>
/// 集成事件上下文接口
/// </summary>
public interface IIntegrationEventContext
{
    /// <summary>
    /// 获取集成事件列表
    /// </summary>
    /// <returns></returns>
    IList<IIntegrationEvent> GetEvents();
}