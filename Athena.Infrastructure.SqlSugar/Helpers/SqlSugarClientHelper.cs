using Athena.Infrastructure.SqlSugar.CAPs.Extends.Models;

namespace Athena.Infrastructure.SqlSugar.Helpers;

/// <summary>
/// 
/// </summary>
public static class SqlSugarClientHelper
{
    /// <summary>
    /// 自动同步Cap消息表
    /// </summary>
    /// <param name="client"></param>
    /// <param name="version"></param>
    public static void AutoSyncCapMessageTable(ISqlSugarClient client, string version = "v1")
    {
        client.CodeFirst.InitTables(new[]
        {
            typeof(Published),
            typeof(Received),
            typeof(Lock)
        });

        // 添加锁
        var key1 = $"publish_retry_{version}";
        // 如果不存在则添加
        var any1 = client.Queryable<Lock>()
            .Where(p => p.Key == key1)
            .Any();
        if (!any1)
        {
            client.Insertable(new Lock
            {
                Key = key1,
                Instance = "",
                LastLockTime = DateTime.MinValue
            }).ExecuteCommand();
        }

        var key2 = $"received_retry_{version}";
        // 如果不存在则添加
        var any2 = client.Queryable<Lock>()
            .Where(p => p.Key == key2)
            .Any();
        if (!any2)
        {
            // 添加锁
            client.Insertable(new Lock
            {
                Key = key2,
                Instance = "",
                LastLockTime = DateTime.MinValue
            }).ExecuteCommand();
        }
    }
}