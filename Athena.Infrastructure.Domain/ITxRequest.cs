namespace Athena.Infrastructure.Domain;

/// <summary>
/// 实现此接口将开启自动事务及支持领域事件和集成事件
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface ITxRequest<out TResponse> : IRequest<TResponse>, ITransactionRequest
{
}