using Athena.Infrastructure.Event.IntegrationEvents;

namespace Athena.Infrastructure.Event.Interfaces;

/// <summary>
/// 消息处理器
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IMessageHandler<in T> : IIntegratedEventHandler<T> where T : IIntegrationEvent
{
}