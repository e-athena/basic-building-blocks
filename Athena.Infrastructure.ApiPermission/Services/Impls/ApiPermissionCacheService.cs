namespace Athena.Infrastructure.ApiPermission.Services.Impls;

/// <summary>
/// 权限缓存服务接口实现类
/// </summary>
public class ApiPermissionCacheService : IApiPermissionCacheService
{
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 权限缓存服务接口实现类
    /// </summary>
    /// <param name="cacheManager"></param>
    public ApiPermissionCacheService(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    /// <summary>
    /// 获取资源列表
    /// </summary>
    /// <param name="appId">APPId</param>
    /// <param name="identificationId">标识Id</param>
    /// <returns></returns>
    public IList<string> Get(string? appId, string identificationId)
    {
        var key = GetCacheKey(appId, identificationId);
        var result = _cacheManager.Get<IList<string>>(key);
        return result ?? new List<string>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="identificationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<string>> GetAsync(string? appId, string identificationId,
        CancellationToken cancellationToken = default)
    {
        var key = GetCacheKey(appId, identificationId);
        var result = await _cacheManager.GetAsync<IList<string>>(key, cancellationToken);
        return result ?? new List<string>();
    }

    /// <summary>
    /// 设置资源列表
    /// </summary>
    /// <param name="appId">APPId</param>
    /// <param name="identificationId">标识Id</param>
    /// <param name="permissions">权限列表</param>
    public void Set(string? appId, string identificationId, IList<string> permissions)
    {
        var key = GetCacheKey(appId, identificationId);
        _cacheManager.Set(key, permissions);
    }

    /// <summary>
    /// 设置资源
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="identificationId"></param>
    /// <param name="permissions"></param>
    /// <param name="timeSpan"></param>
    public void Set(string? appId, string identificationId, IList<string> permissions, TimeSpan timeSpan)
    {
        var key = GetCacheKey(appId, identificationId);
        _cacheManager.Set(key, permissions, timeSpan);
    }

    /// <summary>
    /// 设置资源
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="identificationId"></param>
    /// <param name="permissions"></param>
    /// <param name="cancellationToken"></param>
    public async Task SetAsync(string? appId, string identificationId, IList<string> permissions,
        CancellationToken cancellationToken = default)
    {
        var key = GetCacheKey(appId, identificationId);
        await _cacheManager.SetAsync(key, permissions, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="identificationId"></param>
    /// <param name="permissions"></param>
    /// <param name="timeSpan"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task SetAsync(string? appId, string identificationId, IList<string> permissions, TimeSpan timeSpan,
        CancellationToken cancellationToken = default)
    {
        var key = GetCacheKey(appId, identificationId);
        await _cacheManager.SetAsync(key, permissions, timeSpan, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="identificationId"></param>
    public void Remove(string? appId, string identificationId)
    {
        var key = GetCacheKey(appId, identificationId);
        _cacheManager.Remove(key);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="identificationId"></param>
    /// <param name="cancellationToken"></param>
    public async Task RemoveAsync(string? appId, string identificationId,
        CancellationToken cancellationToken = default)
    {
        var key = GetCacheKey(appId, identificationId);
        await _cacheManager.RemoveAsync(key, cancellationToken);
    }

    /// <summary>
    /// 获取缓存KEY
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="identificationId"></param>
    /// <returns></returns>
    private static string GetCacheKey(string? appId, string identificationId)
    {
        return string.IsNullOrEmpty(appId) ? $"identity:{identificationId}" : $"identity:{appId}:{identificationId}";
    }
}