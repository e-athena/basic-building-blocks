using Athena.Infrastructure.Event.DomainEvents;

namespace Athena.Infrastructure.Event.Interfaces;

/// <summary>
/// 领域事件处理器
/// </summary>
public interface IDomainEventHandler<in T> : INotificationHandler<T> where T : IDomainEvent
{
}