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
        var entityTypes = new List<Type>();
        foreach (var type in assembly.GetExportedTypes())
        {
            entityTypes.AddRange(type.GetCustomAttributes()
                .Where(p => p.GetType() == typeof(TableAttribute) ||
                            p.GetType() == typeof(FreeSql.DataAnnotations.TableAttribute))
                .Select(_ => type));
        }

        var freeSql = Build<IEventStorageFreeSql>(connectionString, dataType, true);
        // 处理索引
        IndexHandle(freeSql, entityTypes.ToArray());

        services.AddSingleton(freeSql);
        services.AddSingleton<IEventStreamQueryService, FreeSqlEventStreamQueryService>();
        return services;
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

    private static void IndexHandle(IFreeSql freeSql, Type[] entityTypes)
    {
        freeSql.CodeFirst.SyncStructure(entityTypes);

        // 处理索引
        // 读取types中带有IndexAttribute的类
        var indexTypes = entityTypes
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
}