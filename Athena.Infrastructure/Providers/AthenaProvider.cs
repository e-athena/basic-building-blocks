namespace Athena.Infrastructure.Providers;

/// <summary>
/// 
/// </summary>
public class AthenaProvider : IAthenaProvider
{
    /// <summary>
    /// 服务提供程序实例
    /// </summary>
    public static IServiceProvider? Provider { get; set; }

    /// <summary>
    /// 默认日志
    /// </summary>
    public static ILogger? DefaultLog { get; set; }

    /// <summary>
    /// 获取服务实例
    /// </summary>
    public static TService? GetService<TService>() where TService : class
    {
        if (Provider == null)
        {
            throw new ArgumentNullException(nameof(Provider), "请使用app.UseAthenaProvider方法注册服务提供程序");
        }

        return Provider.GetService(typeof(TService)) as TService;
    }

    /// <summary>
    /// 获取日志
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ILogger<T>? GetLogger<T>()
    {
        return GetService<ILoggerFactory>()?.CreateLogger<T>();
    }

    /// <summary>
    /// 获取日志
    /// </summary>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ILogger? GetLogger(string categoryName)
    {
        return GetService<ILoggerFactory>()?.CreateLogger(categoryName);
    }
}