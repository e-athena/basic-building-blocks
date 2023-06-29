using Microsoft.Extensions.Logging;

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
}