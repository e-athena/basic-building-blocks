namespace Athena.Infrastructure.FreeSql.Tenants;

/// <summary>
/// 默认的租户实现
/// </summary>
public class DefaultTenantService : ITenantService
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    public DefaultTenantService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// 读取租户信息
    /// </summary>
    /// <param name="tenantCode"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    public async Task<TenantInfo?> GetAsync(string tenantCode, string? appId)
    {
        if (string.IsNullOrEmpty(tenantCode))
        {
            return null;
        }

        var connectionStrings = _configuration.GetSection("ConnectionStrings");
        var connectionStringsChildren = connectionStrings.GetChildren();

        var single = connectionStringsChildren
            .FirstOrDefault(p => string.Equals(p.Key, tenantCode, StringComparison.CurrentCultureIgnoreCase));

        if (single == null)
        {
            return null;
        }

        var (dbType, connectionString) = DbTypeHelper.GetDataTypeAndConnectionString(single.Value);

        return await Task.FromResult(new TenantInfo
        {
            ConnectionString = connectionString,
            DbKey = tenantCode,
            DataType = (int) dbType,
            IsolationLevel = TenantIsolationLevel.Independent
        });
    }
}