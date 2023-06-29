using System.Collections.Concurrent;

namespace Athena.Infrastructure.ApiPermission.Helpers;

/// <summary>
/// 权限帮助类
/// </summary>
public static class ApiPermissionHelper
{
    /// <summary>
    /// 读取API接口权限资源
    /// </summary>
    /// <param name="assembly">默认读取当前的程序集</param>
    /// <returns></returns>
    public static List<ResourceTreeInfo> GetResources<TType>(Assembly? assembly = null)
    {
        if (assembly == null)
        {
            assembly = typeof(TType).Assembly;
        }

        return GetResources(assembly);
    }

    // 缓存资源
    private static readonly ConcurrentDictionary<string, List<ResourceTreeInfo>> ResourceCache = new();

    /// <summary>
    /// 读取API接口权限资源
    /// </summary>
    /// <param name="assembly">默认读取当前的程序集</param>
    /// <returns></returns>
    public static List<ResourceTreeInfo> GetResources(Assembly assembly)
    {
        var assemblyName = assembly.GetName().Name;

        // 如果缓存中存在，则直接返回
        if (ResourceCache.TryGetValue(assemblyName!, out var cacheResult))
        {
            return cacheResult;
        }

        #region 类级别的权限

        var types1 = assembly.GetExportedTypes()
            .Where(p => p.GetCustomAttributes()
                .Any(c => c.GetType() == typeof(ApiPermissionAuthorizeAttribute))
            )
            .ToList();
        // 
        var result1 = SummaryHelper.GetSummaryByClasses(types1, assemblyName!);

        #endregion

        #region 方法级别的权限

        var types = assembly.GetExportedTypes()
            .Where(p => !types1.Contains(p))
            .Where(p => p.GetCustomAttributes()
                .Any(c => c.GetType() == typeof(AuthorizeAttribute))
            )
            .ToList();

        var result = SummaryHelper.GetSummaryByMethods(types, assemblyName!);

        #endregion

        var list = result
            .Union(result1)
            .Select(p => new ResourceTreeInfo(p.Label, p.Value, p.Key)
            {
                Extend = p.Extend,
                Children = p.Children?
                    .Select(c => new ResourceTreeInfo(c.Label, c.Value, c.Key)
                    {
                        Extend = c.Extend
                    })
                    .ToList()
            })
            .Where(p => p.Children?.Count > 0)
            .ToList();

        // 添加到缓存
        ResourceCache.TryAdd(assemblyName!, list);
        return list;
    }

    /// <summary>
    /// 读取API接口权限资源
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    public static List<ResourceTreeInfo> GetResources(string assemblyName)
    {
        return GetResources(Assembly.Load(assemblyName));
    }

    /// <summary>
    /// 读取API接口权限资源
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static string GetResourcesForJavaScript(Assembly assembly)
    {
        var result = GetResources(assembly);
        return GetResourcesForJavaScript(result);
    }

    /// <summary>
    /// 读取API接口权限资源
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static string GetResourcesForJavaScript<TType>(Assembly? assembly = null)
    {
        var result = GetResources<TType>(assembly);
        return GetResourcesForJavaScript(result);
    }

    /// <summary>
    /// 读取API接口权限资源
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private static string GetResourcesForJavaScript(IEnumerable<ResourceTreeInfo> list)
    {
        var temp = new StringBuilder();
        temp.AppendLine("export default {");
        foreach (var item in list)
        {
            var controllerName = item.Key.Replace("Controller", "");
            controllerName = controllerName[..1].ToLower() + controllerName[1..];
            temp.AppendLine($"    /** {item.Label} */");
            temp.AppendLine("    " + controllerName + ": {");
            var index = 0;
            foreach (var child in item.Children ?? new List<ResourceTreeInfo>())
            {
                var actionName = child.Extend == string.Empty
                    ? $"{controllerName}{index}"
                    : child
                        .Extend
                        .Split("_")[1];
                actionName = actionName[..1].ToLower() + actionName[1..];
                temp.AppendLine($"        /** {child.Label} */");
                temp.AppendLine("        " + actionName + ": " + "'" + child.Key + "',");
                index++;
            }

            temp.AppendLine("    },");
        }

        temp.AppendLine("};");
        return temp.ToString();
    }
}