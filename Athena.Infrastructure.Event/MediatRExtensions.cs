// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class MediatRExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="types"></param>
    /// <typeparam name="TType"></typeparam>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    public static IServiceCollection AddCustomMediatR<TType>(
        this IServiceCollection services,
        IList<Type>? types = null)
    {
        var list = new List<Type>
        {
            typeof(TType)
        };
        if (types is {Count: > 0})
        {
            list.AddRange(types);
        }

        services.AddMediatR(list.ToArray());

        return services;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    public static IServiceCollection AddCustomMediatR(this IServiceCollection services)
    {
        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly(),
        };
        services.AddMediatR(assemblies);
        return services;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    public static IServiceCollection AddCustomMediatR(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddMediatR(assemblies);
        return services;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    public static IServiceCollection AddCustomMediatR(
        this IServiceCollection services,
        Action<MediatRServiceConfiguration>? configuration,
        params Assembly[] assemblies)
    {
        services.AddMediatR(configuration, assemblies);
        return services;
    }

    /// <summary>
    /// Asynchronously send a request to a single handler
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="mediator"></param>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
    public static Task<TResponse> SendAsync<TResponse>(
        this IMediator mediator,
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        return mediator.Send(request, cancellationToken);
    }
}