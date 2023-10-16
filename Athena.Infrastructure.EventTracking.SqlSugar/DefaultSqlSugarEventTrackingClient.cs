namespace Athena.Infrastructure.EventTracking.SqlSugar;

public class DefaultSqlSugarEventTrackingClient : SqlSugarScope, ISqlSugarEventTrackingClient
{
    public DefaultSqlSugarEventTrackingClient(ConnectionConfig config) : base(config)
    {
    }

    public DefaultSqlSugarEventTrackingClient(List<ConnectionConfig> configs) : base(configs)
    {
    }

    public DefaultSqlSugarEventTrackingClient(ConnectionConfig config, Action<SqlSugarClient> configAction) : base(config, configAction)
    {
    }

    public DefaultSqlSugarEventTrackingClient(List<ConnectionConfig> configs, Action<SqlSugarClient> configAction) : base(configs, configAction)
    {
    }
}