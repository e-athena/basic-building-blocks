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
    /// <param name="sqlSugarClient"></param>
    /// <param name="entityTypes"></param>
    public static void SyncStructure(this ISqlSugarClient sqlSugarClient, params Type[] entityTypes)
    {
        // 添加CAP的表
        SqlSugarClientHelper.AutoSyncCapMessageTable(sqlSugarClient);
        sqlSugarClient.CodeFirst.InitTables(typeof(OrganizationalUnitAuth));
        sqlSugarClient.CodeFirst.InitTables(entityTypes);
    }

    /// <summary>
    /// 同步表结构
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    /// <param name="assemblyKeyword"></param>
    public static void SyncStructure(this ISqlSugarClient sqlSugarClient, string? assemblyKeyword = null)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeyword);
        SyncStructure(sqlSugarClient, assemblies);
    }

    /// <summary>
    /// 同步表结构
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    /// <param name="assemblyKeywords"></param>
    public static void SyncStructure(this ISqlSugarClient sqlSugarClient, params string[] assemblyKeywords)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeywords);
        SyncStructure(sqlSugarClient, assemblies);
    }

    /// <summary>
    /// 同步表结构
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    /// <param name="assemblies"></param>
    public static void SyncStructure(this ISqlSugarClient sqlSugarClient, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            SyncStructure(sqlSugarClient, assembly);
        }
    }

    /// <summary>
    /// 同步表结构
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    /// <param name="assembly"></param>
    public static void SyncStructure(this ISqlSugarClient sqlSugarClient, Assembly assembly)
    {
        var tableAssemblies = new List<Type>();
        foreach (var type in assembly.GetExportedTypes())
        {
            tableAssemblies.AddRange(type.GetCustomAttributes()
                .Where(p => p.GetType() == typeof(TableAttribute) || p.GetType() == typeof(SugarTable))
                .Select(_ => type));
        }

        SyncStructure(sqlSugarClient, tableAssemblies.ToArray());
    }

    /// <summary>
    /// 同步表结构
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    public static void SyncStructure<TType>(this ISqlSugarClient sqlSugarClient)
    {
        SyncStructure(sqlSugarClient, typeof(TType).Assembly);
    }

    #endregion

    #region SqlSugar

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomSqlSugar(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionStringByEnv();
        return services.AddCustomSqlSugar(connectionString, null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomSqlSugar(
        this IServiceCollection services,
        IConfiguration configuration,
        DbType? dataType)
    {
        var connectionString = configuration.GetConnectionStringByEnv();
        return services.AddCustomSqlSugar(
            connectionString,
            dataType
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString"></param>
    /// <param name="dataType"></param>
    /// <returns></returns>
    private static IServiceCollection AddCustomSqlSugar(
        this IServiceCollection services,
        string connectionString,
        DbType? dataType)
    {
        // 注册SqlSugar
        services.AddSingleton<ISqlSugarClient>(_ =>
        {
            var sqlSugar = new SqlSugarScope(SqlSugarBuilderHelper
                .GetConnectionConfig(
                    Constant.DefaultMainTenant,
                    connectionString,
                    dataType
                ));
            return sqlSugar;
        });
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(SqlSugarGlobalTransactionBehavior<,>));
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
            options.UseSqlSugar();
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