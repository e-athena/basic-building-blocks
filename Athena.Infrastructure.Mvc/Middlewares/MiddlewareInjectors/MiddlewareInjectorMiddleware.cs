namespace Athena.Infrastructure.Mvc.Middlewares.MiddlewareInjectors;

/// <summary>
/// 中间件注入器中间件
/// </summary>
public class MiddlewareInjectorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApplicationBuilder _builder;
    private readonly MiddlewareInjectorOptions _options;
    private RequestDelegate? _subPipeline;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="next"></param>
    /// <param name="builder"></param>
    /// <param name="options"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public MiddlewareInjectorMiddleware(RequestDelegate next, IApplicationBuilder builder,
        MiddlewareInjectorOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public Task Invoke(HttpContext httpContext)
    {
        var injector = _options.GetInjector();
        if (injector != null)
        {
            // 添加多个中间件，如果重复添加，会抛出异常。
            var builder = _builder.New();
            injector(builder);
            builder.Run(_next);
            _subPipeline = builder.Build();
        }

        if (_subPipeline != null)
        {
            return _subPipeline(httpContext);
        }

        return _next(httpContext);
    }
}