using Athena.Infrastructure.EventStorage;
using Athena.Infrastructure.EventStorage.Models;
using Athena.Infrastructure.EventStorage.SqlSugar;

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
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomEventStorage(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "EventStorage")
    {
        try
        {
            var config = configuration.GetConfig<EventStorageOptions>(sectionName);
            return services.AddCustomEventStorage(config);
        }
        catch (Exception)
        {
            // ignored
            return services;
        }
    }

    /// <summary>
    /// 添加事件跟踪
    /// </summary>
    /// <param name="services"></param>
    /// <param name="eventStorageOptions"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns></returns>
    public static IServiceCollection AddCustomEventStorage(
        this IServiceCollection services,
        EventStorageOptions eventStorageOptions
    )
    {
        if (string.IsNullOrEmpty(eventStorageOptions.ConnectionString))
        {
            throw new ArgumentNullException(nameof(eventStorageOptions.ConnectionString), "事件存储的数据库连接字符串不能为空");
        }

        var (dataType, connectionString) =
            DbTypeHelper.GetDataTypeAndConnectionString(eventStorageOptions.ConnectionString);
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "事件存储的数据库连接字符串不能为空");
        }

        // 配置Options
        services.Configure<EventStorageOptions>(options =>
        {
            options.ConnectionString = eventStorageOptions.ConnectionString;
            options.Enabled = eventStorageOptions.Enabled;
        });

        const string dbKey = "eventStorage";
        // 注册SqlSugar
        services.AddSingleton<ISqlSugarEventStorageClient>(_ =>
        {
            var sqlSugar = new DefaultSqlSugarEventStorageClient(SqlSugarBuilderHelper
                .GetConnectionConfig(
                    dbKey,
                    connectionString,
                    dataType
                ));
            sqlSugar.CodeFirst.InitTables(typeof(EventStream));
            const string trackIndexName = "IX_EventStream_AggregateRootId_EventId_Version";
            // 检查是否存在
            var isExists1 = sqlSugar.DbMaintenance.IsAnyIndex(trackIndexName);
            // 如果不存在则添加
            if (!isExists1)
            {
                // 添加索引
                sqlSugar.DbMaintenance
                    .CreateIndex(
                        sqlSugar.DbMaintenance.Context.EntityMaintenance.GetTableName(typeof(EventStream)),
                        new[]
                        {
                            "AggregateRootId",
                            "EventId",
                            "Version"
                        },
                        trackIndexName
                    );
            }

            return sqlSugar;
        });
        services.AddSingleton<IEventStreamQueryService, SqlSugarEventStreamQueryService>();
        return services;
    }
}