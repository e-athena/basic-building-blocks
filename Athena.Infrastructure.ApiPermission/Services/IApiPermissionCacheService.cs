namespace Athena.Infrastructure.ApiPermission.Services;

/// <summary>
/// 权限缓存服务接口
/// </summary>
public interface IApiPermissionCacheService
{
    /// <summary>
    /// 获取资源列表
    /// </summary>
    /// <param name="appId">APPId</param>
    /// <param name="identificationId">标识Id</param>
    /// <returns></returns>
    IList<string> Get(string? appId, string identificationId);

    /// <summary>
    /// 获取资源列表
    /// </summary>
    /// <param name="appId">APPId</param>
    /// <param name="identificationId">标识Id</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IList<string>> GetAsync(string? appId, string identificationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置资源列表
    /// </summary>
    /// <param name="appId">APPId</param>
    /// <param name="identificationId">标识Id</param>
    /// <param name="permissions">资源列表</param>
    void Set(string? appId, string identificationId, IList<string> permissions);

    /// <summary>
    /// 设置资源列表
    /// </summary>
    /// <param name="appId">APPId</param>
    /// <param name="identificationId">标识Id</param>
    /// <param name="permissions">资源列表</param>
    /// <param name="timeSpan">有效期至</param>
    void Set(string? appId, string identificationId, IList<string> permissions, TimeSpan timeSpan);

    /// <summary>
    /// 设置资源列表
    /// </summary>
    /// <param name="appId">APPId</param>
    /// <param name="identificationId">标识Id</param>
    /// <param name="permissions">资源列表</param>
    /// <param name="cancellationToken"></param>
    Task SetAsync(string? appId, string identificationId, IList<string> permissions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置资源列表
    /// </summary>
    /// <param name="appId">APPId</param>
    /// <param name="identificationId">标识Id</param>
    /// <param name="permissions">资源列表</param>
    /// <param name="timeSpan">有效期至</param>
    /// <param name="cancellationToken"></param>
    Task SetAsync(string? appId, string identificationId, IList<string> permissions, TimeSpan timeSpan,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 移除资源列表
    /// </summary>
    /// <param name="appId">APPId</param>
    /// <param name="identificationId">标识Id</param>
    void Remove(string? appId, string identificationId);

    /// <summary>
    /// 移除资源列表
    /// </summary>
    /// <param name="appId">APPId</param>
    /// <param name="identificationId">标识Id</param>
    /// <param name="cancellationToken"></param>
    Task RemoveAsync(string? appId, string identificationId, CancellationToken cancellationToken = default);
}