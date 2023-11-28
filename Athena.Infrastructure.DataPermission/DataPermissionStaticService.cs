using Athena.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Athena.Infrastructure.DataPermission;

/// <summary>
/// 数据权限静态服务接口实现类
/// </summary>
public class DataPermissionStaticService : IDataPermissionStaticService
{
    private readonly IConfiguration _configuration;

    /// <summary>
    ///
    /// </summary>
    /// <param name="configuration"></param>
    public DataPermissionStaticService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="assemblyKeyword"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<Models.DataPermission> GetList(string appId, string? assemblyKeyword = null)
    {
        var keywords = GetAssemblyKeywords(assemblyKeyword ?? string.Empty);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
        var list = new List<Models.DataPermission>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(DataPermissionHelper.GetList(assembly, appId));
        }

        return list;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<Models.DataPermission> GetList(string appId, params string[] assemblyKeywords)
    {
        var keywords = GetAssemblyKeywords(assemblyKeywords);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
        var list = new List<Models.DataPermission>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(DataPermissionHelper.GetList(assembly, appId));
        }

        return list;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="permissions"></param>
    /// <param name="assemblyKeyword"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<Models.DataPermission> GetList(string appId, IList<Models.DataPermission>? permissions,
        string? assemblyKeyword = null)
    {
        var keywords = GetAssemblyKeywords(assemblyKeyword ?? string.Empty);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
        var list = new List<Models.DataPermission>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(DataPermissionHelper.GetList(assembly, appId, permissions));
        }

        return list;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="permissions"></param>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<Models.DataPermission> GetList(string appId, IList<Models.DataPermission>? permissions,
        params string[] assemblyKeywords)
    {
        var keywords = GetAssemblyKeywords(assemblyKeywords);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
        var list = new List<Models.DataPermission>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(DataPermissionHelper.GetList(assembly, appId, permissions));
        }

        return list;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="permissions"></param>
    /// <param name="assemblyKeyword"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IList<DataPermissionGroup> GetGroupList(string appId, IList<Models.DataPermission>? permissions = null,
        string? assemblyKeyword = null)
    {
        var keywords = GetAssemblyKeywords(assemblyKeyword ?? string.Empty);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
        var list = new List<DataPermissionGroup>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(DataPermissionHelper.GetGroupList(assembly, appId, permissions));
        }

        return list;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="permissions"></param>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IList<DataPermissionGroup> GetGroupList(string appId, IList<Models.DataPermission>? permissions = null,
        params string[] assemblyKeywords)
    {
        var keywords = GetAssemblyKeywords(assemblyKeywords);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
        var list = new List<DataPermissionGroup>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(DataPermissionHelper.GetGroupList(assembly, appId, permissions));
        }

        return list;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="assemblyKeyword"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<DataPermissionTree> GetTreeList(string appId, string? assemblyKeyword = null)
    {
        var keywords = GetAssemblyKeywords(assemblyKeyword ?? string.Empty);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
        var list = new List<DataPermissionTree>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(DataPermissionHelper.GetTreeList(assembly, appId));
        }

        return list;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<DataPermissionTree> GetTreeList(string appId, params string[] assemblyKeywords)
    {
        var keywords = GetAssemblyKeywords(assemblyKeywords);
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
        var list = new List<DataPermissionTree>();
        foreach (var assembly in assemblies)
        {
            list.AddRange(DataPermissionHelper.GetTreeList(assembly, appId));
        }

        return list;
    }

    private string[] GetAssemblyKeywords(params string[] sourceKeywords)
    {
        // 去掉空值
        var keywords = sourceKeywords.Where(p => !string.IsNullOrEmpty(p)).ToList();
        // 读取配置DataPermissionAssembly:Keyword
        var assemblyKeyword = _configuration.GetEnvValue<string>("Module:DataPermissionAssembly:Keyword");
        // 如果不为空，添加到关键字列表
        if (!string.IsNullOrEmpty(assemblyKeyword))
        {
            keywords.Add(assemblyKeyword);
        }

        // 读取配置DataPermissionAssembly:Keywords
        var assemblyKeywords = _configuration.GetEnvValues<string>("Module:DataPermissionAssembly:Keywords");
        // 如果不为空，添加到关键字列表
        if (assemblyKeywords is not null && assemblyKeywords.Length > 0)
        {
            keywords.AddRange(assemblyKeywords);
        }

        return keywords.ToArray();
    }
}