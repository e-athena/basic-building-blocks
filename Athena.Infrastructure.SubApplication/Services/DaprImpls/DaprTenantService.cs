namespace Athena.Infrastructure.SubApplication.Services.DaprImpls;

/// <summary>
/// 租户服务
/// </summary>
[Component]
public class DaprTenantService : ITenantService
{
    private readonly ILogger<DaprTenantService> _logger;
    private readonly ICacheManager _cacheManager;
    private readonly DaprClient _daprClient;
    private readonly ServiceCallConfig _config;

    public DaprTenantService(
        ILoggerFactory loggerFactory,
        ICacheManager cacheManager,
        IOptions<ServiceCallConfig> options, DaprClient daprClient)
    {
        _cacheManager = cacheManager;
        _logger = loggerFactory.CreateLogger<DaprTenantService>();
        _daprClient = daprClient;
        _config = options.Value;
    }

    /// <summary>
    /// 读取租户信息
    /// </summary>
    /// <param name="tenantCode"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    public async Task<TenantInfo?> GetAsync(string tenantCode, string? appId)
    {
        var cacheKey = $"tenant:{tenantCode}";
        if (!string.IsNullOrEmpty(appId))
        {
            cacheKey += ":" + appId;
        }

        return await _cacheManager.GetOrCreateAsync(cacheKey, async () =>
        {
            var methodName = $"/api/Util/GetTenantInfo?tenantCode={tenantCode}&appId={appId}";

            var result = await _daprClient
                .InvokeMethodAsync<ApiResult<TenantInfo>>(HttpMethod.Get, _config.AppId, methodName);

            if (!result.Success)
            {
                throw FriendlyException.Of("读取租户信息失败", result.Message);
            }

            var data = result.Data;

            if (data == null)
            {
                throw FriendlyException.Of("读取租户信息失败");
            }

            if (data.IsolationLevel == TenantIsolationLevel.Shared)
            {
                return new TenantInfo
                {
                    ConnectionString = string.Empty,
                    DbKey = tenantCode,
                    IsolationLevel = data.IsolationLevel
                };
            }

            var connectionString = SecurityHelper.Decrypt(data.ConnectionString);
            if (connectionString != null)
            {
                return new TenantInfo
                {
                    ConnectionString = connectionString,
                    DbKey = tenantCode,
                    IsolationLevel = data.IsolationLevel
                };
            }

            _logger.LogWarning("租户连接字符串解密失败");
            throw FriendlyException.Of("读取租户信息失败");
        }, TimeSpan.FromDays(1));
    }
}