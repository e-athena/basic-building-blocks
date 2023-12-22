// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 扩展类
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 添加事件跟踪
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomEventTracking(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionStringByEnv();
        return string.IsNullOrEmpty(connectionString) ? services : services.AddCustomEventTracking(connectionString);
    }

    /// <summary>
    /// 添加事件跟踪
    /// </summary>
    /// <param name="services"></param>
    /// <param name="sourceConnectionString"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomEventTracking(
        this IServiceCollection services,
        string sourceConnectionString
    )
    {
        if (string.IsNullOrEmpty(sourceConnectionString))
        {
            throw new ArgumentNullException(nameof(sourceConnectionString), "事件追踪存储的数据库连接字符串不能为空");
        }

        var (dataType, connectionString) = DbTypeHelper.GetDataTypeAndConnectionString(sourceConnectionString);
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "事件追踪存储的数据库连接字符串不能为空");
        }

        var assembly = Assembly.Load("Athena.Infrastructure.EventTracking");
        var types = assembly
            .GetExportedTypes()
            .Where(t => t
                .GetCustomAttributes()
                .Any(p =>
                    p.GetType() == typeof(TableAttribute) ||
                    p.GetType() == typeof(FreeSql.DataAnnotations.TableAttribute)
                )
            ).ToArray();
        var freeSql = FreeSqlBuilderHelper.Build<IEventTrackingFreeSql>(connectionString, dataType, true);
        if (types.Length > 0)
        {
            var entityTypes = types.ToDictionary<Type, Type, string?>(type => type, _ => null);
            // 处理索引
            IndexHelper.Create(freeSql, entityTypes);
        }

        services.AddSingleton(freeSql);
        services.AddSingleton<ITrackService, TrackService>();
        services.AddSingleton<ITrackStorageService, TrackStorageService>();
        services.AddSingleton<ITrackConfigService, TrackConfigService>();
        return services;
    }

    /// <summary>
    /// 读取连接字符串
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <returns></returns>
    private static string? GetConnectionStringByEnv(
        this IConfiguration configuration,
        string configVariable = "EventTracking",
        string envVariable = "EVENT_TRACKING_CONNECTION_STRING")
    {
        var connectionString = configuration.GetConnectionString(configVariable);
        var env = Environment.GetEnvironmentVariable(envVariable);
        if (!string.IsNullOrEmpty(env))
        {
            connectionString = env;
        }

        return connectionString;
    }
}