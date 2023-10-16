namespace Athena.Infrastructure.Logger.SqlSugar;

public class DefaultSqlSugarLoggerClient : SqlSugarScope, ISqlSugarLoggerClient
{
    public DefaultSqlSugarLoggerClient(ConnectionConfig config) : base(config)
    {
    }

    public DefaultSqlSugarLoggerClient(List<ConnectionConfig> configs) : base(configs)
    {
    }

    public DefaultSqlSugarLoggerClient(ConnectionConfig config, Action<SqlSugarClient> configAction) : base(config, configAction)
    {
    }

    public DefaultSqlSugarLoggerClient(List<ConnectionConfig> configs, Action<SqlSugarClient> configAction) : base(configs, configAction)
    {
    }
}