namespace Athena.Infrastructure.ApiPermission.Services.Impls;

/// <summary>
/// API接口权限服务实现类
/// </summary>
public class ApiPermissionService : IApiPermissionService
{
    private readonly IConfiguration _configuration;

    /// <summary>
    ///
    /// </summary>
    /// <param name="configuration"></param>
    public ApiPermissionService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    #region FrontEndRouting

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="assemblyKeyword"></param>
    /// <returns></returns>
    public IList<MenuTreeInfo> GetFrontEndRoutingResources(string appId, string? assemblyKeyword = null)
    {
        var keywords = GetAssemblyKeywords(assemblyKeyword ?? string.Empty);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
        var list = new List<MenuTreeInfo>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(GetFrontEndRoutingResources(assembly, appId));
        }

        return list;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    public IList<MenuTreeInfo> GetFrontEndRoutingResources(string appId, params string[] assemblyKeywords)
    {
        var keywords = GetAssemblyKeywords(assemblyKeywords);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="codes"></param>
    /// <param name="appId"></param>
    /// <param name="assemblyKeyword"></param>
    /// <returns></returns>
    public IList<MenuTreeInfo> GetPermissionFrontEndRoutingResources(IList<string> codes, string appId,
        string? assemblyKeyword = null)
    {
        var keywords = GetAssemblyKeywords(assemblyKeyword ?? string.Empty);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
        var list = new List<MenuTreeInfo>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(GetPermissionFrontEndRoutingResources(assembly, codes, appId));
        }

        return list;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="codes"></param>
    /// <param name="appId"></param>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    public IList<MenuTreeInfo> GetPermissionFrontEndRoutingResources(IList<string> codes, string appId,
        params string[] assemblyKeywords)
    {
        var keywords = GetAssemblyKeywords(assemblyKeywords);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
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


    /// <summary>
    /// 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="assemblyKeyword"></param>
    /// <returns></returns>
    public IEnumerable<ResourceCodeInfo> GetResourceCodeInfos(string appId, string? assemblyKeyword = null)
    {
        var keywords = GetAssemblyKeywords(assemblyKeyword ?? string.Empty);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
        var list = new List<ResourceCodeInfo>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(GetResourceCodeInfos(assembly, appId));
        }

        return list;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    public IEnumerable<ResourceCodeInfo> GetResourceCodeInfos(string appId, params string[] assemblyKeywords)
    {
        var keywords = GetAssemblyKeywords(assemblyKeywords);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="assemblyKeyword"></param>
    /// <returns></returns>
    public IList<string> GetDuplicateResourceCodes(string appId, string? assemblyKeyword = null)
    {
        var keywords = GetAssemblyKeywords(assemblyKeyword ?? string.Empty);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
        var list = new List<string>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(GetDuplicateResourceCodes(assembly, appId));
        }

        return list;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    public IList<string> GetDuplicateResourceCodes(string appId, params string[] assemblyKeywords)
    {
        var keywords = GetAssemblyKeywords(assemblyKeywords);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="assemblyKeyword"></param>
    /// <returns></returns>
    public bool HasDuplicateResourceCodes(string appId, string? assemblyKeyword = null)
    {
        var keywords = GetAssemblyKeywords(assemblyKeyword ?? string.Empty);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
        var list = new List<bool>();
        foreach (var assembly in assemblies)
        {
            list.Add(HasDuplicateResourceCodes(assembly, appId));
        }

        return list.Any(p => p);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    public bool HasDuplicateResourceCodes(string appId, params string[] assemblyKeywords)
    {
        var keywords = GetAssemblyKeywords(assemblyKeywords);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
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

    private string[] GetAssemblyKeywords(params string[] sourceKeywords)
    {
        // 去掉空值
        var keywords = sourceKeywords.Where(p => !string.IsNullOrEmpty(p)).ToList();
        // 读取配置ApiPermissionAssembly:Keyword
        var assemblyKeyword = _configuration.GetEnvValue<string>("Module:ApiPermissionAssembly:Keyword");
        // 如果不为空，添加到关键字列表
        if (!string.IsNullOrEmpty(assemblyKeyword))
        {
            keywords.Add(assemblyKeyword);
        }

        // 读取配置ApiPermissionAssembly:Keywords
        var assemblyKeywords = _configuration.GetEnvValues<string>("Module:ApiPermissionAssembly:Keywords");
        // 如果不为空，添加到关键字列表
        if (assemblyKeywords is not null && assemblyKeywords.Length > 0)
        {
            keywords.AddRange(assemblyKeywords);
        }

        return keywords.ToArray();
    }
}