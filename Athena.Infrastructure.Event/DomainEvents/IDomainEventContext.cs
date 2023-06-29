namespace Athena.Infrastructure.Event.DomainEvents;

/// <summary>
/// 领域事件上下文接口
/// </summary>
public interface IDomainEventContext
{
    /// <summary>
    /// 获取领域事件列表
    /// </summary>
    /// <returns></returns>
    IList<IDomainEvent> GetEvents();
}