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

        var assembly = Assembly.Load("Athena.Infrastructure.EventStorage");

        var types = assembly
            .GetExportedTypes()
            .Where(t => t
                .GetCustomAttributes()
                .Any(p =>
                    p.GetType() == typeof(TableAttribute) ||
                    p.GetType() == typeof(FreeSql.DataAnnotations.TableAttribute)
                )
            ).ToArray();
        var freeSql = FreeSqlBuilderHelper.Build<IEventStorageFreeSql>(connectionString, dataType, true);
        if (types.Length > 0)
        {
            var entityTypes = types.ToDictionary<Type, Type, string?>(type => type, _ => null);
            // 处理索引
            IndexHelper.Create(freeSql, entityTypes);
        }

        services.AddSingleton(freeSql);
        services.AddSingleton<IEventStreamQueryService, FreeSqlEventStreamQueryService>();
        return services;
    }
}