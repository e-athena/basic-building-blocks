using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Athena.Infrastructure.Domain.Attributes;
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

            var type = typeof(EventStream);
            sqlSugar.CodeFirst.InitTables(type);

            #region 处理索引

            // 读取索引
            var indexAttributes = type.GetCustomAttributes()
                .Where(p => p is IndexAttribute)
                .Cast<IndexAttribute>()
                .ToList();

            // 读取表名
            var tableName = type.GetCustomAttributes()
                .Where(p => p is TableAttribute or SugarTable)
                .Select(p => p is TableAttribute tableAttribute ? tableAttribute.Name : ((SugarTable) p).TableName)
                .FirstOrDefault();

            foreach (var indexAttribute in indexAttributes)
            {
                // 读取索引名
                var indexName = indexAttribute.Name ??
                                $"IX_{tableName}_{string.Join("_", indexAttribute.PropertyNames)}";
                if (indexName.Length > 64)
                {
                    indexName = indexName[..64];
                }

                // 检查是否存在
                var isExists = sqlSugar.DbMaintenance.IsAnyIndex(indexName);
                // 如果不存在则添加
                if (!isExists)
                {
                    // 添加索引
                    sqlSugar.DbMaintenance.CreateIndex(
                        tableName,
                        indexAttribute.PropertyNames.ToArray(),
                        indexName,
                        indexAttribute.IsUnique
                    );
                }
            }

            #endregion

            return sqlSugar;
        });
        services.AddSingleton<IEventStreamQueryService, SqlSugarEventStreamQueryService>();
        return services;
    }
}