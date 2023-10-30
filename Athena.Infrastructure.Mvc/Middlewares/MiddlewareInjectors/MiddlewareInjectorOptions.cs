namespace Athena.Infrastructure.Mvc.Middlewares.MiddlewareInjectors;

/// <summary>
/// 中间件注入器选项
/// </summary>
public class MiddlewareInjectorOptions
{
    private Action<IApplicationBuilder>? _injector;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    public void InjectMiddleware(Action<IApplicationBuilder>? builder)
    {
        Interlocked.Exchange(ref _injector, builder);
    }

    internal Action<IApplicationBuilder>? GetInjector()
    {
        return Interlocked.Exchange(ref _injector, null);
    }
}