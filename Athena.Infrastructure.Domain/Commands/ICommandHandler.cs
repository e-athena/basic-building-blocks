namespace Athena.Infrastructure.Domain.Commands;

/// <summary>
/// ICommandHandler
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public interface ICommandHandler<in TRequest> : IRequestHandler<TRequest, string> where TRequest : IRequest<string>
{
}

/// <summary>
/// ICommandHandler
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface ICommandHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<string>, IRequest<TResponse>
{
}