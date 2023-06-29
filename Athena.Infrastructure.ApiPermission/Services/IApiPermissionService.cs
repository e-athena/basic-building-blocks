namespace Athena.Infrastructure.ApiPermission.Services;

/// <summary>
/// API接口权限服务
/// </summary>
public interface IApiPermissionService
{
    #region FrontEndRouting

    /// <summary>
    /// 读取权限资源
    /// </summary>
    /// <remarks>只读取有权限的资源</remarks>
    /// <param name="assembly">程序集</param>
    /// <param name="codes"></param>
    /// <param name="appId">应用ID</param>
    /// <returns></returns>
    IList<MenuTreeInfo> GetPermissionFrontEndRoutingResources(
        Assembly assembly,
        IList<string> codes,
        string appId
    );

    /// <summary>
    /// 读取权限资源
    /// </summary>
    /// <remarks>只读取有权限的资源</remarks>
    /// <param name="list">原始数据</param>
    /// <param name="codes"></param>
    /// <param name="appId">应用ID</param>
    /// <returns></returns>
    IList<MenuTreeInfo> GetPermissionFrontEndRoutingResources(
        IList<MenuTreeInfo> list,
        IList<string> codes,
        string appId
    );

    /// <summary>
    /// 读取资源
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="appId">应用ID</param>
    /// <returns></returns>
    IList<MenuTreeInfo> GetFrontEndRoutingResources(
        Assembly assembly,
        string appId
    );

    #endregion

    /// <summary>
    /// 读取资源代码信息列表
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="appId">应用ID</param>
    /// <returns></returns>
    IList<ResourceCodeInfo> GetResourceCodeInfos(Assembly assembly, string appId);

    /// <summary>
    /// 获取重名的资源代码
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="appId">应用ID</param>
    /// <returns></returns>
    IList<string> GetDuplicateResourceCodes(Assembly assembly, string appId);

    /// <summary>
    /// 检查是否有重名的资源代码
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="appId">应用ID</param>
    /// <returns></returns>
    bool HasDuplicateResourceCodes(Assembly assembly, string appId);
}