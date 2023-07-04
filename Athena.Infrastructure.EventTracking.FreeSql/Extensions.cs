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
        return services.AddCustomEventTracking(connectionString);
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
        const string dbKey = "eventTracking";
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "事件追踪存储的数据库连接字符串不能为空");
        }

        var freeSqlCloud = new FreeSqlCloud();
        freeSqlCloud.Register(dbKey, () =>
        {
            // build
            var freeSql = new FreeSqlBuilder()
                .UseConnectionString(dataType, connectionString)
                .UseMonitorCommand(cmd => Console.WriteLine(cmd.CommandText))
                .UseAutoSyncStructure(true)
                .Build();
            freeSql.Aop.ConfigEntityProperty += (_, args) =>
            {
                if (args.Property.PropertyType.IsEnum)
                {
                    args.ModifyResult.MapType = typeof(int);
                }
            };
            return freeSql;
        });
        freeSqlCloud.EntitySteering = (_, e) =>
        {
            if (e.EntityType == typeof(Track))
            {
                e.DBKey = dbKey;
            }
        };

        services.AddSingleton(freeSqlCloud);
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
    private static string GetConnectionStringByEnv(
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