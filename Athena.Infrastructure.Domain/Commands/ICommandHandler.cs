namespace Athena.Infrastructure.Domain;

/// <summary>
/// 兼容ENode的ICommandHandler
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public interface ICommandHandler<in TRequest> : IRequestHandler<TRequest, string> where TRequest : IRequest<string>
{
}