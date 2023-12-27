using System.ComponentModel.DataAnnotations.Schema;
using Athena.Infrastructure.DataAnnotations.Schema;

namespace Athena.Infrastructure.FreeSqlHelper;

/// <summary>
/// 索引帮助类
/// </summary>
public static class IndexHelper
{
    /// <summary>
    /// 创建索引
    /// </summary>
    /// <param name="freeSql"></param>
    /// <param name="dictionary">key:entityType,value:customTableName</param>
    public static void Create(IFreeSql freeSql, IDictionary<Type, string?> dictionary)
    {
        // 先处理没有自定义表名的
        freeSql.CodeFirst.SyncStructure(dictionary
            .Where(c => string.IsNullOrEmpty(c.Value))
            .Select(c => c.Key)
            .ToArray()
        );
        foreach (var kv in dictionary.Where(c => !string.IsNullOrEmpty(c.Value)))
        {
            freeSql.CodeFirst.SyncStructure(kv.Key, kv.Value);
        }

        var entityTypes = dictionary.Keys.ToList();
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
            var tableName = GetTableName(type);

            // 如果有自定义表名则使用自定义表名
            if (dictionary.TryGetValue(type, out var value) && !string.IsNullOrEmpty(value))
            {
                tableName = value;
            }

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

                try
                {
                    freeSql.Ado.ExecuteNonQuery(createIndexSql);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }
            }
        }
    }

    /// <summary>
    /// 读取表名
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetTableName(Type type)
    {
        var tableName = type.GetCustomAttributes()
            .Where(p => p is TableAttribute or FreeSql.DataAnnotations.TableAttribute)
            .Select(p => p is TableAttribute tableAttribute
                ? tableAttribute.Name
                : ((FreeSql.DataAnnotations.TableAttribute) p).Name)
            .FirstOrDefault();

        return tableName ?? type.Name;
    }
}