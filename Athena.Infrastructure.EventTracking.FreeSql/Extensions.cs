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

        services.AddSingleton(Build<IEventTrackingFreeSql>(connectionString, dataType, true));
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

    /// <summary>
    /// 构建
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="dataType">数据库类型，为空时根据链接字符串自动读取</param>
    /// <param name="isAutoSyncStructure">是否自动同步表结构</param>
    /// <param name="actionAop"></param>
    /// <param name="actionSqlBuilder"></param>
    /// <returns></returns>
    private static IFreeSql<TMark> Build<TMark>(string connectionString,
        DataType? dataType = null,
        bool isAutoSyncStructure = false,
        Action<IAop>? actionAop = null,
        Action<FreeSqlBuilder>? actionSqlBuilder = null)
    {
        if (dataType == null)
        {
            var res = DbTypeHelper.GetDataTypeAndConnectionString(connectionString);
            dataType = res.dataType;
            connectionString = res.connectionString;
        }

        var freeSqlBuilder = new FreeSqlBuilder()
            .UseConnectionString(dataType.Value, connectionString)
            .UseMonitorCommand(null, (cmd, traceLog) =>
            {
                if (AthenaProvider.DefaultLog == null ||
                    !AthenaProvider.DefaultLog.IsEnabled(Logging.LogLevel.Debug))
                {
                    return;
                }

                // 打印日志
                Console.WriteLine("----------------------------------SQL监控开始----------------------------------");
                Console.WriteLine($"{cmd.Connection?.Database ?? string.Empty} {traceLog}");
                Console.WriteLine("----------------------------------SQL监控结束----------------------------------");
            })
            // 自动同步实体结构到数据库
            .UseAutoSyncStructure(isAutoSyncStructure);

        actionSqlBuilder?.Invoke(freeSqlBuilder);
        // build
        var freeSql = freeSqlBuilder.Build<TMark>();
        freeSql.Aop.CommandAfter += (_, args) =>
        {
            if (args.ElapsedMilliseconds <= 800)
            {
                return;
            }

            // 打印日志
            AthenaProvider.DefaultLog?.LogWarning("SQL监控，执行时间超过800毫秒：{Sql}", args.Log.Replace("\r\n", " "));
        };
        freeSql.Aop.ConfigEntityProperty += (_, e) =>
        {
            if (e.Property.PropertyType.IsEnum)
            {
                e.ModifyResult.MapType = typeof(int);
            }

            if (Nullable.GetUnderlyingType(e.Property.PropertyType)?.IsEnum == true)
            {
                e.ModifyResult.MapType = typeof(int?);
            }
        };
        actionAop?.Invoke(freeSql.Aop);

        return freeSql;
    }
}