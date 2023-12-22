namespace Athena.Infrastructure.SqlSugar.Helpers;

/// <summary>
/// 索引帮助类
/// </summary>
public static class IndexHelper
{
    /// <summary>
    /// 创建索引
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    /// <param name="dictionary"></param>
    public static void Create(ISqlSugarClient sqlSugarClient, IDictionary<Type, string?> dictionary)
    {
        // 先处理没有自定义表名的
        sqlSugarClient.CodeFirst.InitTables(dictionary
            .Where(c => string.IsNullOrEmpty(c.Value))
            .Select(c => c.Key)
            .ToArray()
        );
        foreach (var kv in dictionary.Where(c => !string.IsNullOrEmpty(c.Value)))
        {
            sqlSugarClient.CodeFirst.As(kv.Key, kv.Value).InitTables();
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
                if (indexName.Length > 64)
                {
                    indexName = indexName[..64];
                }

                // 检查是否存在
                var isExists = sqlSugarClient.DbMaintenance.IsAnyIndex(indexName);
                // 如果不存在则添加
                if (!isExists)
                {
                    // 添加索引
                    sqlSugarClient.DbMaintenance.CreateIndex(
                        tableName,
                        indexAttribute.PropertyNames.ToArray(),
                        indexName,
                        indexAttribute.IsUnique
                    );
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
            .Where(p => p is TableAttribute or SugarTable)
            .Select(p => p is TableAttribute tableAttribute ? tableAttribute.Name : ((SugarTable) p).TableName)
            .FirstOrDefault();

        return tableName ?? type.Name;
    }
}