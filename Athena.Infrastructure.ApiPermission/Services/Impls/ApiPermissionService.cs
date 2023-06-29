namespace Athena.Infrastructure.ApiPermission.Services.Impls;

/// <summary>
/// API接口权限服务实现类
/// </summary>
public class ApiPermissionService : IApiPermissionService
{
    #region FrontEndRouting

    /// <summary>
    /// 读取菜单资源
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="appId">应用ID</param>
    /// <returns></returns>
    public IList<MenuTreeInfo> GetFrontEndRoutingResources(Assembly assembly, string appId)
    {
        return FrontEndRoutingHelper.GetResources(assembly, appId);
    }

    /// <summary>
    /// 读取权限资源
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="codes"></param>
    /// <param name="appId">应用ID</param>
    /// <returns></returns>
    public IList<MenuTreeInfo> GetPermissionFrontEndRoutingResources(
        Assembly assembly,
        IList<string> codes,
        string appId
    )
    {
        return FrontEndRoutingHelper.GetPermissionResources(assembly, codes, appId);
    }

    /// <summary>
    /// 读取权限资源
    /// </summary>
    /// <param name="list"></param>
    /// <param name="codes"></param>
    /// <param name="appId">应用ID</param>
    /// <returns></returns>
    public IList<MenuTreeInfo> GetPermissionFrontEndRoutingResources(
        IList<MenuTreeInfo> list,
        IList<string> codes,
        string appId
    )
    {
        return FrontEndRoutingHelper.GetPermissionResources(list, codes, appId);
    }

    #endregion

    /// <summary>
    /// 读取资源代码信息列表
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    public IList<ResourceCodeInfo> GetResourceCodeInfos(Assembly assembly, string appId)
    {
        return FrontEndRoutingHelper.GetResourceCodeInfos(assembly, appId);
    }

    /// <summary>
    /// 获取重名的资源代码
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IList<string> GetDuplicateResourceCodes(Assembly assembly, string appId)
    {
        var codeInfos = GetResourceCodeInfos(assembly, appId);
        var duplicateCodes = codeInfos
            .Select(p => p.Key)
            .GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();
        return duplicateCodes;
    }

    /// <summary>
    /// 检查是否有重名的资源代码
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool HasDuplicateResourceCodes(Assembly assembly, string appId)
    {
        return GetDuplicateResourceCodes(assembly, appId).Count > 0;
    }
}