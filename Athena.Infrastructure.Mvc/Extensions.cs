using Athena.Infrastructure.Auth.Configs;
using Athena.Infrastructure.Mvc;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adds services for controllers to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    public static IMvcBuilder AddCustomController(
        this IServiceCollection services,
        Action<MvcOptions>? configure = null
    )
    {
        return services
            .AddControllers(options =>
            {
                options.AddCustomApiResultFilter();
                options.AddCustomApiExceptionFilter();
                configure?.Invoke(options);
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                // 自定义验证失败返回结果
                options.InvalidModelStateResponseFactory = context =>
                {
                    var traceId = Activity.Current != null
                        ? Activity.Current.TraceId.ToString()
                        : context.HttpContext.TraceIdentifier;
                    var message = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(v => v.ErrorMessage)
                        .Aggregate("", (current, error) => current + $"{error};");
                    context.HttpContext.Response.StatusCode = 400;
                    return new JsonResult(new CustomBadRequestResult
                    {
                        Success = false,
                        Message = message,
                        Errors = context.ModelState.Keys.Select(key => new Dictionary<string, string[]>
                        {
                            {
                                key,
                                context.ModelState[key]!.Errors.Select(x => x.ErrorMessage).ToArray()
                            }
                        }).ToList(),
                        StatusCode = context.HttpContext.Response.StatusCode,
                        TraceId = traceId
                    });
                };
            });
    }

    /// <summary>
    /// 添加API异常过滤器
    /// </summary>
    /// <param name="options"></param>
    public static void AddCustomApiExceptionFilter(this MvcOptions options)
    {
        options.Filters.Add<ApiExceptionFilterAttribute>();
    }

    /// <summary>
    /// 添加API结果统一处理过滤器
    /// </summary>
    /// <param name="options"></param>
    public static void AddCustomApiResultFilter(this MvcOptions options)
    {
        options.Filters.Add<ApiResultFilterAttribute>();
    }
}