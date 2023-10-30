// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 
/// </summary>
public static class SignalRBuilderExtensions
{
    /// <summary>
    /// 自定义的SignalR
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="setupAction"></param>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapCustomSignalR(this IEndpointRouteBuilder builder,
        Action<IEndpointRouteBuilder>? setupAction = null)
    {
        builder.MapHub<NoticeHub>("/hubs/notice");
        builder.MapHub<EventHub>("/hubs/event");
        setupAction?.Invoke(builder);
        return builder;
    }
}