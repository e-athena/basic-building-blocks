// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 添加基础认证
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomBasicAuth(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddOptions();
        var config = configuration.GetConfig<BasicAuthConfig>("BasicAuthConfig", "BASIC_AUTH_CONFIG");
        services.Configure<BasicAuthConfig>(cfg =>
        {
            cfg.UserName = config.UserName;
            cfg.Password = config.Password;
        });
        return services;
    }

    /// <summary>
    /// 添加基础认证
    /// </summary>
    /// <param name="services"></param>
    /// <param name="userName">用户名</param>
    /// <param name="password">密码</param>
    /// <returns></returns>
    public static IServiceCollection AddCustomBasicAuth(
        this IServiceCollection services,
        string userName,
        string password
    )
    {
        services.Configure<BasicAuthConfig>(cfg =>
        {
            cfg.UserName = userName;
            cfg.Password = password;
        });
        return services;
    }

    /// <summary>
    /// Adds services for controllers to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services"></param>
    public static IMvcBuilder AddCustomController(this IServiceCollection services)
    {
        return services.AddCustomController(null);
    }

    /// <summary>
    /// Adds services for controllers to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    public static IMvcBuilder AddCustomController(
        this IServiceCollection services,
        Action<MvcOptions>? configure)
    {
        return services.AddControllers(options =>
        {
            options.AddCustomApiResultFilter();
            options.AddCustomApiExceptionFilter();
            configure?.Invoke(options);
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

    /// <summary>
    /// 添加服务组件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomServiceComponent(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        foreach (var type in assemblies.SelectMany(assembly => assembly.GetTypes().Where(IsServiceComponent)))
        {
            if (type.GetCustomAttributes(typeof(ComponentAttribute), false)
                    .FirstOrDefault() is not ComponentAttribute t)
            {
                continue;
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                switch (t.LifeStyle)
                {
                    case LifeStyle.Scoped:
                        services.AddScoped(interfaceType, type);
                        break;
                    case LifeStyle.Transient:
                        services.AddTransient(interfaceType, type);
                        break;
                    case LifeStyle.Singleton:
                        services.AddSingleton(interfaceType, type);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        return services;
    }

    /// <summary>
    /// Check whether a type is a component type.
    /// </summary>
    private static bool IsServiceComponent(Type type)
    {
        return type is {IsClass: true, IsAbstract: false} &&
               type.GetCustomAttributes(typeof(ComponentAttribute), false).Any();
    }
}