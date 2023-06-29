using Athena.Infrastructure.Event.IntegrationEvents;

namespace Athena.Infrastructure.Event.Interfaces;

/// <summary>
/// 集成事件处理器
/// </summary>
public interface IIntegratedEventHandler<in T> : ICapSubscribe where T : IIntegrationEvent
{
    /// <summary>
    /// 事件处理
    /// </summary>
    /// <param name="payload">参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    Task HandleAsync(T payload, CancellationToken cancellationToken);
}