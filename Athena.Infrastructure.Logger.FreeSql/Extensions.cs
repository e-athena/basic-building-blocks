// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 扩展类
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 添加日志
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomStorageLogger(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionStringByEnv();
        return services.AddCustomStorageLogger(connectionString);
    }

    /// <summary>
    /// 添加日志
    /// </summary>
    /// <param name="services"></param>
    /// <param name="sourceConnectionString"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomStorageLogger(
        this IServiceCollection services,
        string sourceConnectionString)
    {
        if (string.IsNullOrEmpty(sourceConnectionString))
        {
            throw new ArgumentNullException(nameof(sourceConnectionString), "日志存储的数据库连接字符串不能为空");
        }

        var (dataType, connectionString) = DbTypeHelper.GetDataTypeAndConnectionString(sourceConnectionString);
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "日志存储的数据库连接字符串不能为空");
        }

        var assembly = Assembly.Load("Athena.Infrastructure.Logger");
        var types = assembly
            .GetExportedTypes()
            .Where(t => t
                .GetCustomAttributes()
                .Any(p =>
                    p.GetType() == typeof(TableAttribute) ||
                    p.GetType() == typeof(FreeSql.DataAnnotations.TableAttribute)
                )
            ).ToArray();
        var freeSql = FreeSqlBuilderHelper.Build<ILoggerFreeSql>(connectionString, dataType, true);
        if (types.Length > 0)
        {
            var entityTypes = types.ToDictionary<Type, Type, string?>(type => type, _ => null);
            // 处理索引
            IndexHelper.Create(freeSql, entityTypes);
        }

        services.AddSingleton(freeSql);
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddSingleton<ILoggerStorageService, LoggerStorageService>();
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
        string configVariable = "LoggerCenter",
        string envVariable = "LOGGER_CENTER_CONNECTION_STRING")
    {
        var connectionString = configuration.GetConnectionString(configVariable);
        var env = Environment.GetEnvironmentVariable(envVariable);
        if (!string.IsNullOrEmpty(env))
        {
            connectionString = env;
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = configuration.GetEnvValue<string>("Module:LoggerCenter:ConnectionString");
        }

        if (connectionString == null)
        {
            throw new ArgumentNullException(nameof(connectionString),
                "日志存储的数据库连接字符串不能为空, 请在 appsettings.json 或环境变量中配置 LoggerCenter");
        }

        return connectionString;
    }
}