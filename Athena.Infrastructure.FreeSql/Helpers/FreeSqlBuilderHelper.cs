using Athena.Infrastructure.DataAnnotations.Schema;
using Athena.Infrastructure.FreeSql.CAPs.Extends;

namespace Athena.Infrastructure.FreeSql.Helpers;

/// <summary>
/// FreeSqlBuilder帮助类
/// </summary>
public static class FreeSqlBuilderHelper
{
    /// <summary>
    /// 构建
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="dataType">数据库类型，为空时根据链接字符串自动读取</param>
    /// <param name="isAutoSyncStructure">是否自动同步表结构</param>
    /// <param name="actionAop"></param>
    /// <param name="actionSqlBuilder"></param>
    /// <returns></returns>
    public static IFreeSql Build(string connectionString,
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
                if (AthenaProvider.DefaultLog == null || !AthenaProvider.DefaultLog.IsEnabled(LogLevel.Debug))
                {
                    return;
                }

                // cap的表不处理
                if (cmd.CommandText.Contains(CapConstant.PublishedTableName) ||
                    cmd.CommandText.Contains(CapConstant.ReceivedTableName) ||
                    cmd.CommandText.Contains(CapConstant.LockTableName))
                {
                    return;
                }

                // 打印日志
                Console.WriteLine("----------------------------------SQL监控开始----------------------------------");
                Console.WriteLine($"{cmd.Connection?.Database ?? string.Empty} {traceLog}");
                Console.WriteLine("----------------------------------SQL监控结束----------------------------------");
                // AthenaProvider.DefaultLog?.LogDebug("SQL监控：{Sql}", traceLog);
                // AthenaProvider.DefaultLog?.LogDebug("CommandText：{CommandText}", cmd.CommandText);
            })
            // 自动同步实体结构到数据库
            .UseAutoSyncStructure(isAutoSyncStructure);

        actionSqlBuilder?.Invoke(freeSqlBuilder);
        // build
        var freeSql = freeSqlBuilder.Build();
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

            if (e.Property
                    .GetCustomAttributes(typeof(RowVersionAttribute), false)
                    .FirstOrDefault() is RowVersionAttribute)
            {
                e.ModifyResult.IsVersion = true;
            }

            if (e.Property
                    .GetCustomAttributes(typeof(FieldSortAttribute), false)
                    .FirstOrDefault() is FieldSortAttribute)
            {
                e.ModifyResult.Position =
                    (short) -((FieldSortAttribute) e.Property
                            .GetCustomAttributes(typeof(FieldSortAttribute), false)
                            .First(it => it is FieldSortAttribute))
                        .Value;
            }
        };
        actionAop?.Invoke(freeSql.Aop);

        return freeSql;
    }
}