using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ITenant = SqlSugar.ITenant;

namespace Athena.Infrastructure.SqlSugar.Helpers;

/// <summary>
/// SqlSugarBuilder帮助类
/// </summary>
public static class SqlSugarBuilderHelper
{
    /// <summary>
    /// 注册租户
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="connectionString"></param>
    /// <param name="dataType">数据库类型，为空时根据链接字符串自动读取</param>
    /// <param name="tenant"></param>
    /// <returns></returns>
    public static ISqlSugarClient Registry(
        ITenant tenant,
        string tenantId,
        string connectionString,
        DbType? dataType = null)
    {
        tenant.AddConnection(GetConnectionConfig(tenantId, connectionString, dataType));
        return tenant.GetConnectionScope(tenantId);
    }

    /// <summary>
    /// 注册租户
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="connectionString"></param>
    /// <param name="dataType">数据库类型，为空时根据链接字符串自动读取</param>
    /// <param name="client"></param>
    /// <returns></returns>
    public static ISqlSugarClient Registry(
        ISqlSugarClient client,
        string tenantId,
        string connectionString,
        DbType? dataType = null)
    {
        var tenant = client.AsTenant();
        return Registry(tenant, tenantId, connectionString, dataType);
    }

    /// <summary>
    /// 读取链接配置
    /// </summary>
    /// <param name="tenantId">租户ID</param>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="dataType">数据库类型</param>
    /// <returns></returns>
    public static ConnectionConfig GetConnectionConfig(
        string tenantId,
        string connectionString,
        DbType? dataType = null)
    {
        if (dataType == null)
        {
            var res = DbTypeHelper.GetDataTypeAndConnectionString(connectionString);
            dataType = res.dataType;
            connectionString = res.connectionString;
        }

        return new ConnectionConfig
        {
            AopEvents = new AopEvents
            {
                OnDiffLogEvent = it =>
                {
                    //操作前记录  包含： 字段描述 列名 值 表名 表描述
                    var editBeforeData = it.BeforeData;
                    //操作后记录   包含： 字段描述 列名 值  表名 表描述
                    var editAfterData = it.AfterData;
                    var sql = it.Sql;
                    var parameter = it.Parameters;
                    var data = it.BusinessData; // 这边会显示你传进来的对象
                    var time = it.Time;
                    var diffType = it.DiffType; // enum insert 、update and delete  
                    // 打印日志
                    AthenaProvider.DefaultLog?.LogDebug("操作前数据：{@EditBeforeData}", editBeforeData);
                    AthenaProvider.DefaultLog?.LogDebug("操作后数据：{@EditAfterData}", editAfterData);
                    AthenaProvider.DefaultLog?.LogDebug("SQL监控：{Sql}", sql);
                    // ReSharper disable once CoVariantArrayConversion
                    AthenaProvider.DefaultLog?.LogDebug("参数：{@Parameter}", parameter);
                    AthenaProvider.DefaultLog?.LogDebug("业务数据：{@Data}", data);
                    AthenaProvider.DefaultLog?.LogDebug("耗时：{@Time}", time);
                    AthenaProvider.DefaultLog?.LogDebug("操作类型：{@DiffType}", diffType);
                },
                OnLogExecuted = (sql, _) =>
                {
                    // 
                    AthenaProvider.DefaultLog?.LogDebug("执行SQL：{Sql}", sql);
                }
            },
            DbType = dataType.Value,
            ConnectionString = connectionString,
            IsAutoCloseConnection = true,
            ConfigId = tenantId,
            ConfigureExternalServices = new ConfigureExternalServices
            {
                EntityService = (property, column) =>
                {
                    var attributes = property.GetCustomAttributes(true);

                    if (attributes.Any(it => it is NotMappedAttribute) ||
                        (!column.UnderType.FullName!.Contains("System.") && !column.UnderType.IsEnum) ||
                        attributes.Any(it => it is JsonIgnoreAttribute))
                    {
                        column.IsIgnore = true;
                    }


                    if (attributes.Any(it => it is KeyAttribute))
                    {
                        column.IsPrimarykey = true;
                    }

                    if (attributes.Any(it => it is RowVersionAttribute))
                    {
                        column.IsEnableUpdateVersionValidation = true;
                    }

                    if (attributes.Any(it => it is FieldSortAttribute))
                    {
                        column.CreateTableFieldSort =
                            ((FieldSortAttribute) attributes.First(it => it is FieldSortAttribute))
                            .Value;
                    }

                    if (column.IsPrimarykey == false && property.PropertyType.IsGenericType &&
                        property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        column.IsNullable = true;
                    }

                    if (column.IsPrimarykey == false && new NullabilityInfoContext()
                            .Create(property).WriteState is NullabilityState.Nullable)
                    {
                        column.IsNullable = true;
                    }
                },
                EntityNameService = (property, column) =>
                {
                    var attributes = property.GetCustomAttributes(true);

                    if (attributes.Any(it => it is TableAttribute))
                    {
                        column.DbTableName = ((TableAttribute) attributes.First(it => it is TableAttribute))
                            .Name;
                    }
                }
            },
            MoreSettings = new ConnMoreSettings()
            {
                SqliteCodeFirstEnableDefaultValue = true,
                SqliteCodeFirstEnableDescription = true
            }
        };
    }
}