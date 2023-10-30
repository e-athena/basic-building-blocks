// ReSharper disable once CheckNamespace

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 中间件注入器
/// </summary>
public static class MiddlewareInjectorExtensions
{
    /// <summary>
    /// 添加中间件注入器
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomMiddlewareInjector(this IServiceCollection services)
    {
        services.AddSingleton<MiddlewareInjectorOptions>();
        return services;
    }

    /// <summary>
    /// UseCustomMiddlewareInjector
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseCustomMiddlewareInjector(this IApplicationBuilder builder)
    {
        var options = builder.ApplicationServices.GetService<MiddlewareInjectorOptions>();
        if (options == null)
        {
            throw new InvalidOperationException("Please call AddCustomMiddlewareInjector first.");
        }

        return builder.UseMiddleware<MiddlewareInjectorMiddleware>(builder.New(), options);
    }
}