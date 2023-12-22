using Athena.Infrastructure.DataPermission;
using Athena.Infrastructure.QueryFilters;

namespace Athena.Infrastructure.SubApplication.Services.DaprImpls;

/// <summary>
/// 数据权限服务接口实现类
/// </summary>
public class DaprDataPermissionService : DaprServiceBase, IDataPermissionService
{
    private readonly DaprClient _daprClient;
    private readonly ServiceCallConfig _config;
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="daprClient"></param>
    /// <param name="accessor"></param>
    /// <param name="cacheManager"></param>
    public DaprDataPermissionService(IOptions<ServiceCallConfig> options, DaprClient daprClient,
        ISecurityContextAccessor accessor, ICacheManager cacheManager) : base(accessor)
    {
        _daprClient = daprClient;
        _cacheManager = cacheManager;
        _config = options.Value;
    }

    /// <summary>
    /// 读取策略查询过滤组
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="resourceKey"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<List<QueryFilterGroup>> GetPolicyQueryFilterGroupsAsync(string userId, string resourceKey,
        string? appId)
    {
        // Key
        var key = string.Format(CacheConstant.UserPolicyFilterGroupQuery, userId, resourceKey);
        // 过期时间
        var expireTime = TimeSpan.FromMinutes(30);
        return await _cacheManager.GetOrCreateAsync(key, QueryFunc, expireTime) ?? new List<QueryFilterGroup>();

        async Task<List<QueryFilterGroup>> QueryFunc()
        {
            var methodName =
                $"/api/SubApplication/GetUserPolicyQueryFilterGroups?userId={userId}&resourceKey={resourceKey}&appId={appId}";
            var result = await _daprClient
                .InvokeMethodAsync<ApiResult<List<QueryFilterGroup>>>(HttpMethod.Get, _config.AppId,
                    GetMethodName(methodName)).ConfigureAwait(false);

            return result.Data ?? new List<QueryFilterGroup>();
        }
    }

    /// <summary>
    /// 读取用户所在组织机构Id
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public List<string> GetUserOrganizationIds(string userId, string? appId)
    {
        return GetUserOrganizationIdsAsync(userId, appId).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 读取用户所在组织机构Id
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<List<string>> GetUserOrganizationIdsAsync(string userId, string? appId)
    {
        // Key
        var key = string.Format(CacheConstant.UserOrganizationKey, userId);
        // 过期时间
        var expireTime = TimeSpan.FromMinutes(30);
        return await _cacheManager.GetOrCreateAsync(key, QueryFunc, expireTime) ?? new List<string>();

        async Task<List<string>> QueryFunc()
        {
            var methodName = $"/api/SubApplication/GetUserOrganizationIds?userId={userId}&appId={appId}";
            var result = await _daprClient
                .InvokeMethodAsync<ApiResult<List<string>>>(HttpMethod.Get, _config.AppId,
                    GetMethodName(methodName)).ConfigureAwait(false);

            return result.Data ?? new List<string>();
        }
    }

    /// <summary>
    /// 读取用户所在组织机构及子机构Id
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public List<string> GetUserOrganizationIdsTree(string userId, string? appId)
    {
        return GetUserOrganizationIdsTreeAsync(userId, appId).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 读取用户所在组织机构及子机构Id
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<List<string>> GetUserOrganizationIdsTreeAsync(string userId, string? appId)
    {
        // Key
        var key = string.Format(CacheConstant.UserOrganizationsKey, userId);
        // 过期时间
        var expireTime = TimeSpan.FromMinutes(30);
        return await _cacheManager.GetOrCreateAsync(key, QueryFunc, expireTime) ?? new List<string>();

        async Task<List<string>> QueryFunc()
        {
            var methodName = $"/api/SubApplication/GetUserOrganizationIdsTree?userId={userId}&appId={appId}";
            var result = await _daprClient
                .InvokeMethodAsync<ApiResult<List<string>>>(HttpMethod.Get, _config.AppId,
                    GetMethodName(methodName)).ConfigureAwait(false);
            return result.Data ?? new List<string>();
        }
    }

    /// <summary>
    /// 读取用户角色的数据范围列表
    /// </summary>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public List<DataPermission.Models.DataPermission> GetUserDataScopes(string userId, string? appId)
    {
        return GetUserDataScopesAsync(userId, appId).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 读取用户角色的数据范围列表
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<List<DataPermission.Models.DataPermission>> GetUserDataScopesAsync(string userId, string? appId)
    {
        // Key
        var key = string.Format(CacheConstant.UserDataScopesKey, userId);
        // 过期时间
        var expireTime = TimeSpan.FromMinutes(30);
        return await _cacheManager.GetOrCreateAsync(key, QueryFunc, expireTime) ??
               new List<DataPermission.Models.DataPermission>();

        async Task<List<DataPermission.Models.DataPermission>> QueryFunc()
        {
            var methodName = $"/api/SubApplication/GetUserDataScopes?userId={userId}&appId={appId}";
            var result = await _daprClient
                .InvokeMethodAsync<ApiResult<List<DataPermission.Models.DataPermission>>>(HttpMethod.Get, _config.AppId,
                    GetMethodName(methodName)).ConfigureAwait(false);
            return result.Data ?? new List<DataPermission.Models.DataPermission>();
        }
    }
}