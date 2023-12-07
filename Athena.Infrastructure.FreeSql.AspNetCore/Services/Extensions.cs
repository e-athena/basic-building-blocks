// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 扩展方法
/// </summary>
public static class Extensions
{
    #region 同步表结构

    /// <summary>
    /// 同步表结构
    /// </summary>
    /// <param name="freeSql"></param>
    public static void SyncStructure<TType>(this IFreeSql freeSql)
    {
        SyncStructure(freeSql, typeof(TType).Assembly);
    }

    /// <summary>
    /// 同步表结构
    /// </summary>
    /// <param name="freeSql"></param>
    /// <param name="assemblyKeyword"></param>
    public static void SyncStructure(this IFreeSql freeSql, string? assemblyKeyword = null)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeyword);
        SyncStructure(freeSql, assemblies);
    }

    /// <summary>
    /// 同步表结构
    /// </summary>
    /// <param name="freeSql"></param>
    /// <param name="assemblyKeywords"></param>
    public static void SyncStructure(this IFreeSql freeSql, params string[] assemblyKeywords)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeywords);
        SyncStructure(freeSql, assemblies);
    }

    /// <summary>
    /// 同步表结构
    /// </summary>
    /// <param name="freeSql"></param>
    /// <param name="assemblies"></param>
    public static void SyncStructure(this IFreeSql freeSql, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            SyncStructure(freeSql, assembly);
        }
    }

    /// <summary>
    /// 同步表结构
    /// </summary>
    /// <param name="freeSql"></param>
    /// <param name="assembly"></param>
    public static void SyncStructure(this IFreeSql freeSql, Assembly assembly)
    {
        var tableAssemblies = new List<Type>();
        foreach (var type in assembly.GetExportedTypes())
        {
            tableAssemblies.AddRange(type.GetCustomAttributes()
                .Where(p => p.GetType() == typeof(TableAttribute) ||
                            p.GetType() == typeof(FreeSql.DataAnnotations.TableAttribute))
                .Select(_ => type));
        }

        SyncStructure(freeSql, tableAssemblies.ToArray());
    }

    /// <summary>
    /// 同步表结构
    /// </summary>
    /// <param name="freeSql"></param>
    /// <param name="entityTypes"></param>
    public static void SyncStructure(this IFreeSql freeSql, params Type[] entityTypes)
    {
        var types = new List<Type>
        {
            typeof(Published),
            typeof(Received),
            typeof(Lock),
            typeof(OrganizationalUnitAuth)
        };
        if (entityTypes.Length > 0)
        {
            types.AddRange(entityTypes);
        }

        // types去重
        types = types.Distinct().ToList();

        freeSql.CodeFirst.SyncStructure(types.ToArray());

        // 处理索引
        // 读取types中带有IndexAttribute的类
        var indexTypes = types
            .Where(p => p.GetCustomAttributes().Any(c => c is IndexAttribute))
            .ToList();

        // 处理索引
        foreach (var type in indexTypes)
        {
            // 读取索引
            var indexAttributes = type.GetCustomAttributes()
                .Where(p => p is IndexAttribute)
                .Cast<IndexAttribute>()
                .ToList();

            // 读取表名
            var tableName = type.GetCustomAttributes()
                .Where(p => p is TableAttribute or FreeSql.DataAnnotations.TableAttribute)
                .Select(p => p is TableAttribute tableAttribute
                    ? tableAttribute.Name
                    : ((FreeSql.DataAnnotations.TableAttribute) p).Name)
                .FirstOrDefault();

            foreach (var indexAttribute in indexAttributes)
            {
                // 读取索引名
                var indexName = indexAttribute.Name ??
                                $"IX_{tableName}_{string.Join("_", indexAttribute.PropertyNames)}";

                // 如果indexName超出64个字符则截取
                if (indexName.Length > 64)
                {
                    indexName = indexName[..64];
                }

                var sql = freeSql.Ado.DataType switch
                {
                    // 适配多数据库
                    // mysql
                    DataType.MySql => $"SHOW INDEX FROM {tableName} WHERE Key_name = '{indexName}'",
                    DataType.SqlServer => $"SELECT * FROM sys.indexes WHERE name = '{indexName}'",
                    // sqlite
                    DataType.Sqlite => $"SELECT * FROM sqlite_master WHERE name = '{indexName}'",
                    // pgsql
                    DataType.PostgreSQL => $"SELECT * FROM pg_indexes WHERE indexname = '{indexName}'",
                    // oracle
                    DataType.Oracle => $"SELECT * FROM user_indexes WHERE index_name = '{indexName}'",
                    _ => $"SHOW INDEX FROM {tableName} WHERE Key_name = '{indexName}'"
                };

                // 检查是否存在
                var isExists = freeSql.Ado.ExecuteScalar(sql) != null;
                // 如果不存在则添加
                if (isExists)
                {
                    continue;
                }

                // 添加索引，
                string createIndexSql;
                switch (freeSql.Ado.DataType)
                {
                    // mysql
                    case DataType.MySql:
                        // indexAttribute.IsUnique
                        createIndexSql =
                            $"ALTER TABLE {tableName} ADD {(indexAttribute.IsUnique ? "UNIQUE" : "")} INDEX {indexName} ({string.Join(",", indexAttribute.PropertyNames)})";
                        break;
                    // sqlite
                    case DataType.Sqlite:
                    // pgsql
                    case DataType.PostgreSQL:
                    // oracle
                    case DataType.Oracle:
                    // sqlserver
                    case DataType.SqlServer:
                        // indexAttribute.IsUnique
                        createIndexSql =
                            $"CREATE {(indexAttribute.IsUnique ? "UNIQUE" : "")} INDEX {indexName} ON {tableName} ({string.Join(",", indexAttribute.PropertyNames)})";
                        break;
                    // 其他数据库
                    default:
                        // indexAttribute.IsUnique
                        createIndexSql =
                            $"ALTER TABLE {tableName} ADD {(indexAttribute.IsUnique ? "UNIQUE" : "")} INDEX {indexName} ({string.Join(",", indexAttribute.PropertyNames)})";
                        break;
                }

                freeSql.Ado.ExecuteNonQuery(createIndexSql);
            }
        }
    }

    #endregion

    #region FreeSql

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomFreeSql(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionStringByEnv();
        return services.AddCustomFreeSql(connectionString, null, false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="isAutoSyncStructure"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomFreeSql(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isAutoSyncStructure)
    {
        var connectionString = configuration.GetConnectionStringByEnv();
        return services.AddCustomFreeSql(connectionString, null, isAutoSyncStructure);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="isAutoSyncStructure"></param>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomFreeSql(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isAutoSyncStructure,
        DataType dataType)
    {
        var connectionString = configuration.GetConnectionStringByEnv();
        return services.AddCustomFreeSql(connectionString, dataType, isAutoSyncStructure);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="isAutoSyncStructure"></param>
    /// <param name="actionAop"></param>
    /// <param name="actionSqlBuilder"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomFreeSql(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isAutoSyncStructure,
        Action<IAop>? actionAop,
        Action<FreeSqlBuilder>? actionSqlBuilder = null)
    {
        var connectionString = configuration.GetConnectionStringByEnv();
        return services.AddCustomFreeSql(
            connectionString,
            null,
            isAutoSyncStructure,
            actionAop,
            actionSqlBuilder
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="isAutoSyncStructure"></param>
    /// <param name="dataType"></param>
    /// <param name="actionAop"></param>
    /// <param name="actionSqlBuilder"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomFreeSql(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isAutoSyncStructure,
        DataType dataType,
        Action<IAop>? actionAop,
        Action<FreeSqlBuilder>? actionSqlBuilder = null)
    {
        var connectionString = configuration.GetConnectionStringByEnv();
        return services.AddCustomFreeSql(
            connectionString,
            dataType,
            isAutoSyncStructure,
            actionAop,
            actionSqlBuilder
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString"></param>
    /// <param name="dataType"></param>
    /// <param name="isAutoSyncStructure"></param>
    /// <returns></returns>
    private static IServiceCollection AddCustomFreeSql(
        this IServiceCollection services,
        string connectionString,
        DataType? dataType,
        bool isAutoSyncStructure)
    {
        return services.AddCustomFreeSql(connectionString, dataType, isAutoSyncStructure, null, null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString"></param>
    /// <param name="dataType"></param>
    /// <param name="isAutoSyncStructure"></param>
    /// <param name="actionAop"></param>
    /// <param name="actionSqlBuilder"></param>
    /// <returns></returns>
    private static IServiceCollection AddCustomFreeSql(
        this IServiceCollection services,
        string connectionString,
        DataType? dataType,
        bool isAutoSyncStructure,
        Action<IAop>? actionAop,
        Action<FreeSqlBuilder>? actionSqlBuilder)
    {
        FreeSqlMultiTenancyManager.Instance.Register(Constant.DefaultMainTenant, () =>
            FreeSqlBuilderHelper.Build(
                connectionString,
                dataType,
                isAutoSyncStructure,
                actionAop,
                actionSqlBuilder
            )
        );

        services.AddSingleton<IFreeSql>(FreeSqlMultiTenancyManager.Instance);
        services.AddSingleton(FreeSqlMultiTenancyManager.Instance);
        services.AddScoped<UnitOfWorkManagerCloud>();
        services.AddScoped<UnitOfWorkManager>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(FreeSqlGlobalTransactionBehavior<,>));
        services.AddScoped<IDomainEventContext, DomainEventContext>();
        services.AddScoped<IIntegrationEventContext, IntegrationEventContext>();
        return services;
    }

    #endregion

    #region 集成事件支持（CAP）

    /// <summary>
    /// 添加集成事件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="capOptions">CAP配置</param>
    /// <param name="assemblyKeyword">程序集关键字</param>
    /// <returns></returns>
    public static IServiceCollection AddCustomIntegrationEvent(
        this IServiceCollection services,
        Action<CapOptions> capOptions,
        string? assemblyKeyword = null)
    {
        return services.AddCustomIntegrationEvent(capOptions,
            AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeyword));
    }

    /// <summary>
    /// 添加集成事件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="capOptions">CAP配置</param>
    /// <param name="assemblyKeywords">程序集关键字</param>
    /// <returns></returns>
    public static IServiceCollection AddCustomIntegrationEvent(
        this IServiceCollection services,
        Action<CapOptions> capOptions,
        params string[] assemblyKeywords)
    {
        return services.AddCustomIntegrationEvent(capOptions,
            AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeywords));
    }

    /// <summary>
    /// 添加集成事件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="capOptions">CAP配置</param>
    /// <param name="capSubscribeAssemblies">订阅端程序集</param>
    /// <returns></returns>
    public static IServiceCollection AddCustomIntegrationEvent(
        this IServiceCollection services,
        Action<CapOptions> capOptions,
        Assembly[]? capSubscribeAssemblies = null)
    {
        services.AddCap(options =>
        {
            options.DefaultGroupName = "default.group";
            capOptions.Invoke(options);
        });
        if (capSubscribeAssemblies != null && capSubscribeAssemblies.Any())
        {
            services.AddCustomIntegrationEventHandler(capSubscribeAssemblies);
        }

        return services;
    }

    /// <summary>
    /// 添加集成事件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="capRedisOptions">Redis配置</param>
    /// <param name="capOptions">CAP配置</param>
    /// <param name="assemblyKeyword">订阅端程序集</param>
    /// <remarks>默认使用Mysql和Redis</remarks>
    /// <returns></returns>
    public static IServiceCollection AddCustomIntegrationEvent(
        this IServiceCollection services,
        Action<CapRedisOptions> capRedisOptions,
        Action<CapOptions>? capOptions = null,
        string? assemblyKeyword = null)
    {
        return services.AddCustomIntegrationEvent(capRedisOptions, capOptions,
            AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeyword));
    }

    /// <summary>
    /// 添加集成事件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="capRedisOptions">Redis配置</param>
    /// <param name="capOptions">CAP配置</param>
    /// <param name="assemblyKeywords">订阅端程序集</param>
    /// <remarks>默认使用Mysql和Redis</remarks>
    /// <returns></returns>
    public static IServiceCollection AddCustomIntegrationEvent(
        this IServiceCollection services,
        Action<CapRedisOptions> capRedisOptions,
        Action<CapOptions>? capOptions = null,
        params string[] assemblyKeywords)
    {
        return services.AddCustomIntegrationEvent(capRedisOptions, capOptions,
            AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeywords));
    }

    /// <summary>
    /// 添加集成事件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="capRedisOptions">Redis配置</param>
    /// <param name="capOptions">CAP配置</param>
    /// <param name="capSubscribeAssemblies">订阅端程序集</param>
    /// <remarks>默认使用Mysql和Redis</remarks>
    /// <returns></returns>
    public static IServiceCollection AddCustomIntegrationEvent(
        this IServiceCollection services,
        Action<CapRedisOptions> capRedisOptions,
        Action<CapOptions>? capOptions = null,
        Assembly[]? capSubscribeAssemblies = null)
    {
        services.AddCap(options =>
        {
            options.UseFreeSql();
            options.UseRedis(capRedisOptions);
            options.DefaultGroupName = "default.group";
            capOptions?.Invoke(options);
        });
        if (capSubscribeAssemblies != null && capSubscribeAssemblies.Any())
        {
            services.AddCustomIntegrationEventHandler(capSubscribeAssemblies);
        }

        return services;
    }

    /// <summary>
    /// 添加集成事件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="assemblyKeyword">订阅端程序集</param>
    /// <remarks>默认使用Mysql和Redis</remarks>
    /// <returns></returns>
    public static IServiceCollection AddCustomIntegrationEvent(
        this IServiceCollection services,
        IConfiguration configuration,
        string? assemblyKeyword = null
    )
    {
        return services.AddCustomIntegrationEvent(configuration,
            AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeyword));
    }

    /// <summary>
    /// 添加集成事件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="assemblyKeywords">订阅端程序集</param>
    /// <remarks>默认使用Mysql和Redis</remarks>
    /// <returns></returns>
    public static IServiceCollection AddCustomIntegrationEvent(
        this IServiceCollection services,
        IConfiguration configuration,
        params string[] assemblyKeywords
    )
    {
        return services.AddCustomIntegrationEvent(configuration,
            AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeywords));
    }

    /// <summary>
    /// 添加集成事件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="capSubscribeAssemblies">订阅端程序集</param>
    /// <remarks>默认使用Mysql和Redis</remarks>
    /// <returns></returns>
    public static IServiceCollection AddCustomIntegrationEvent(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly[]? capSubscribeAssemblies = null
    )
    {
        return services.AddCustomIntegrationEvent(options =>
        {
            var connectionString = configuration.GetCapRedisConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
            {
                options.Configuration = ConfigurationOptions.Parse(connectionString);
            }
        }, null, capSubscribeAssemblies);
    }

    /// <summary>
    /// 添加集成事件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="capOptions">CAP配置</param>
    /// <param name="assemblyKeyword">订阅端程序集</param>
    /// <remarks>默认使用Mysql和Redis</remarks>
    /// <returns></returns>
    public static IServiceCollection AddCustomIntegrationEvent(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<CapOptions> capOptions,
        string? assemblyKeyword = null
    )
    {
        return services.AddCustomIntegrationEvent(configuration, capOptions,
            AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeyword));
    }

    /// <summary>
    /// 添加集成事件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="capOptions">CAP配置</param>
    /// <param name="assemblyKeywords">订阅端程序集</param>
    /// <remarks>默认使用Mysql和Redis</remarks>
    /// <returns></returns>
    public static IServiceCollection AddCustomIntegrationEvent(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<CapOptions> capOptions,
        params string[] assemblyKeywords
    )
    {
        return services.AddCustomIntegrationEvent(configuration, capOptions,
            AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeywords));
    }

    /// <summary>
    /// 添加集成事件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="capOptions">CAP配置</param>
    /// <param name="capSubscribeAssemblies">订阅端程序集</param>
    /// <remarks>默认使用Mysql和Redis</remarks>
    /// <returns></returns>
    public static IServiceCollection AddCustomIntegrationEvent(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<CapOptions> capOptions,
        Assembly[] capSubscribeAssemblies
    )
    {
        return services.AddCustomIntegrationEvent(options =>
        {
            var connectionString = configuration.GetCapRedisConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
            {
                options.Configuration = ConfigurationOptions.Parse(connectionString);
            }
        }, capOptions, capSubscribeAssemblies);
    }

    /// <summary>
    /// 添加集成事件处理器
    /// </summary>
    /// <param name="services"></param>
    /// <param name="capSubscribeAssemblies"></param>
    /// <returns></returns>
    private static void AddCustomIntegrationEventHandler(this IServiceCollection services,
        params Assembly[] capSubscribeAssemblies)
    {
        foreach (var type in capSubscribeAssemblies.SelectMany(assembly => assembly.GetTypes().Where(IsEventHandler)))
        {
            services.AddScoped(type);
        }
    }

    /// <summary>
    /// Check whether a type is a component type.
    /// </summary>
    private static bool IsEventHandler(Type type)
    {
        return type is {IsClass: true, IsAbstract: false} && type.GetInterfaces().Contains(typeof(ICapSubscribe));
    }

    #endregion

    /// <summary>
    /// 读取连接字符串
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns></returns>
    private static string GetConnectionStringByEnv(
        this IConfiguration configuration,
        string configVariable = "Default",
        string envVariable = "CONNECTION_STRING")
    {
        var connectionString = configuration.GetConnectionString(configVariable);
        var env = Environment.GetEnvironmentVariable(envVariable);
        if (!string.IsNullOrEmpty(env))
        {
            connectionString = env;
        }

        if (connectionString == null)
        {
            throw new ArgumentNullException(nameof(connectionString), "读取连接字符串失败");
        }

        return connectionString;
    }

    /// <summary>
    /// 读取CAP连接字符串
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns></returns>
    private static string? GetCapRedisConnectionString(
        this IConfiguration configuration,
        string configVariable = "CAP",
        string envVariable = "CAP")
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