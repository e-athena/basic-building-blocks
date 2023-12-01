namespace Athena.Infrastructure.EventStorage.SqlSugar;


public class DefaultSqlSugarEventStorageClient : SqlSugarScope, ISqlSugarEventStorageClient
{
    public DefaultSqlSugarEventStorageClient(ConnectionConfig config) : base(config)
    {
    }

    public DefaultSqlSugarEventStorageClient(List<ConnectionConfig> configs) : base(configs)
    {
    }

    public DefaultSqlSugarEventStorageClient(ConnectionConfig config, Action<SqlSugarClient> configAction) : base(config, configAction)
    {
    }

    public DefaultSqlSugarEventStorageClient(List<ConnectionConfig> configs, Action<SqlSugarClient> configAction) : base(configs, configAction)
    {
    }
}