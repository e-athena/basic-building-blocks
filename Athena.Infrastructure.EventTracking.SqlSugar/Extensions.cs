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
        const string dbKey = "eventTracking";
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "事件追踪存储的数据库连接字符串不能为空");
        }


        // 注册SqlSugar
        services.AddSingleton<ISqlSugarEventTrackingClient>(_ =>
        {
            var sqlSugar = new DefaultSqlSugarEventTrackingClient(SqlSugarBuilderHelper
                .GetConnectionConfig(
                    dbKey,
                    connectionString,
                    dataType
                ));
            sqlSugar.CodeFirst.InitTables(typeof(Track), typeof(TrackConfig));
            const string trackIndexName = "IX_Track_TraceId";
            // 检查是否存在
            var isExists1 = sqlSugar.DbMaintenance.IsAnyIndex(trackIndexName);
            // 如果不存在则添加
            if (!isExists1)
            {
                // 添加索引
                sqlSugar.DbMaintenance
                    .CreateIndex(
                        sqlSugar.DbMaintenance.Context.EntityMaintenance.GetTableName(typeof(Track)),
                        new[]
                        {
                            "TraceId"
                        },
                        trackIndexName
                    );
            }

            const string trackConfigIndexName = "IX_TrackConfig_ConfigId";
            // 检查是否存在
            var isExists2 = sqlSugar.DbMaintenance.IsAnyIndex(trackConfigIndexName);
            // 如果不存在则添加
            if (!isExists2)
            {
                // 添加索引
                sqlSugar.DbMaintenance
                    .CreateIndex(
                        sqlSugar.DbMaintenance.Context.EntityMaintenance.GetTableName(typeof(TrackConfig)),
                        new[]
                        {
                            "ConfigId"
                        },
                        trackConfigIndexName
                    );
            }

            return sqlSugar;
        });
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