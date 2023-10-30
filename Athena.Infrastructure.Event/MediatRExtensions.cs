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
    /// <param name="configuration"></param>
    /// <param name="types"></param>
    /// <typeparam name="TType"></typeparam>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    public static IServiceCollection AddCustomMediatR<TType>(
        this IServiceCollection services,
        Action<MediatRServiceConfiguration>? configuration,
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

        services.AddMediatR(configuration, list.ToArray());

        return services;
    }

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
    /// <param name="assemblyKeyword"></param>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    public static IServiceCollection AddCustomMediatR(this IServiceCollection services, string? assemblyKeyword = null)
    {
        services.AddMediatR(AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeyword));
        return services;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="assemblyKeyword"></param>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    public static IServiceCollection AddCustomMediatR(this IServiceCollection services,
        Action<MediatRServiceConfiguration>? configuration,
        string? assemblyKeyword = null)
    {
        services.AddMediatR(configuration, AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeyword));
        return services;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    public static IServiceCollection AddCustomMediatR(this IServiceCollection services,
        params string[] assemblyKeywords)
    {
        services.AddMediatR(AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeywords));
        return services;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    public static IServiceCollection AddCustomMediatR(this IServiceCollection services,
        Action<MediatRServiceConfiguration>? configuration,
        params string[] assemblyKeywords)
    {
        services.AddMediatR(configuration, AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeywords));
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

    /// <summary>Asynchronously send a notification to multiple handlers</summary>
    /// <param name="mediator"></param>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation.</returns>
    public static Task PublishAsync(this IMediator mediator, object notification,
        CancellationToken cancellationToken = default)
    {
        return mediator.Publish(notification, cancellationToken);
    }

    /// <summary>Asynchronously send a notification to multiple handlers</summary>
    /// <param name="mediator"></param>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation.</returns>
    public static Task PublishAsync<TNotification>(this IMediator mediator, TNotification notification,
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        return mediator.Publish(notification, cancellationToken);
    }

    /// <summary>Asynchronously send a notification to multiple handlers</summary>
    /// <param name="publisher"></param>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation.</returns>
    public static Task PublishAsync(this IPublisher publisher, object notification,
        CancellationToken cancellationToken = default)
    {
        return publisher.Publish(notification, cancellationToken);
    }

    /// <summary>Asynchronously send a notification to multiple handlers</summary>
    /// <param name="publisher"></param>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation.</returns>
    public static Task PublishAsync<TNotification>(this IPublisher publisher, TNotification notification,
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        return publisher.Publish(notification, cancellationToken);
    }

    /// <summary>
    /// Asynchronously send a request to a single handler
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="sender"></param>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
    public static Task<TResponse> SendAsync<TResponse>(
        this ISender sender,
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        return sender.Send(request, cancellationToken);
    }
}