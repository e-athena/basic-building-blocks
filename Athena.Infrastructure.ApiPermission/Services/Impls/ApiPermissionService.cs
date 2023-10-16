namespace Athena.Infrastructure.ApiPermission.Services.Impls;

/// <summary>
/// API接口权限服务实现类
/// </summary>
public class ApiPermissionService : IApiPermissionService
{
    #region FrontEndRouting

    public IList<MenuTreeInfo> GetFrontEndRoutingResources(string appId, string? assemblyKeyword = null)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeyword);
        var list = new List<MenuTreeInfo>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(GetFrontEndRoutingResources(assembly, appId));
        }

        return list;
    }

    public IList<MenuTreeInfo> GetFrontEndRoutingResources(string appId, params string[] assemblyKeywords)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeywords);
        var list = new List<MenuTreeInfo>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(GetFrontEndRoutingResources(assembly, appId));
        }

        return list;
    }

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

    public IList<MenuTreeInfo> GetPermissionFrontEndRoutingResources(IList<string> codes, string appId,
        string? assemblyKeyword = null)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeyword);
        var list = new List<MenuTreeInfo>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(GetPermissionFrontEndRoutingResources(assembly, codes, appId));
        }

        return list;
    }

    public IList<MenuTreeInfo> GetPermissionFrontEndRoutingResources(IList<string> codes, string appId,
        params string[] assemblyKeywords)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeywords);
        var list = new List<MenuTreeInfo>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(GetPermissionFrontEndRoutingResources(assembly, codes, appId));
        }

        return list;
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


    public IEnumerable<ResourceCodeInfo> GetResourceCodeInfos(string appId, string? assemblyKeyword = null)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeyword);
        var list = new List<ResourceCodeInfo>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(GetResourceCodeInfos(assembly, appId));
        }

        return list;
    }

    public IEnumerable<ResourceCodeInfo> GetResourceCodeInfos(string appId, params string[] assemblyKeywords)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeywords);
        var list = new List<ResourceCodeInfo>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(GetResourceCodeInfos(assembly, appId));
        }

        return list;
    }

    /// <summary>
    /// 读取资源代码信息列表
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    public IEnumerable<ResourceCodeInfo> GetResourceCodeInfos(Assembly assembly, string appId)
    {
        return FrontEndRoutingHelper.GetResourceCodeInfos(assembly, appId);
    }

    public IList<string> GetDuplicateResourceCodes(string appId, string? assemblyKeyword = null)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeyword);
        var list = new List<string>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(GetDuplicateResourceCodes(assembly, appId));
        }

        return list;
    }

    public IList<string> GetDuplicateResourceCodes(string appId, params string[] assemblyKeywords)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeywords);
        var list = new List<string>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(GetDuplicateResourceCodes(assembly, appId));
        }

        return list;
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

    public bool HasDuplicateResourceCodes(string appId, string? assemblyKeyword = null)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeyword);
        var list = new List<bool>();
        foreach (var assembly in assemblies)
        {
            list.Add(HasDuplicateResourceCodes(assembly, appId));
        }

        return list.Any(p => p);
    }

    public bool HasDuplicateResourceCodes(string appId, params string[] assemblyKeywords)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeywords);
        var list = new List<bool>();
        foreach (var assembly in assemblies)
        {
            list.Add(HasDuplicateResourceCodes(assembly, appId));
        }

        return list.Any(p => p);
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