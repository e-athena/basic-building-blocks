namespace Athena.Infrastructure.ApiPermission.Helpers;

/// <summary>
/// 菜单帮助类
/// </summary>
public static class FrontEndRoutingHelper
{
    /// <summary>
    /// 缓存菜单
    /// </summary>
    private static readonly ConcurrentDictionary<string, IList<FrontEndRoutingAttribute>>
        FrontEndRoutingAttributesCache = new();

    /// <summary>
    /// 缓存功能
    /// </summary>
    private static readonly ConcurrentDictionary<string, IList<FunctionInfo>> FunctionInfosCache = new();

    /// <summary>
    /// 读取权限菜单资源
    /// </summary>
    /// <param name="assembly">程序集</param>
    /// <returns></returns>
    private static IList<FrontEndRoutingAttribute> GetFrontEndRoutingAttributes(Assembly assembly)
    {
        var assemblyName = assembly.GetName().Name;
        var list = new List<FrontEndRoutingAttribute>();
        // 读取缓存
        if (FrontEndRoutingAttributesCache.TryGetValue(assemblyName!, out var cacheResult))
        {
            list.AddRange(cacheResult);
        }
        else
        {
            // 读取所有的FrontEndRoutingAttribute
            var types = assembly.GetExportedTypes()
                .Where(p => p.GetCustomAttributes()
                    .Any(c => c.GetType() == typeof(FrontEndRoutingAttribute))
                )
                .ToList();
            XPathNavigator? xmlNavigator = null;
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var files = new DirectoryInfo(path).GetFiles();
            // 找出里面的xml
            var applicationsXml = files
                .Where(n => !string.IsNullOrEmpty(n.Extension))
                .Where(n => assemblyName!.Contains(n.Name.Replace(n.Extension, "")))
                .FirstOrDefault(p => p.Extension.ToLower() == ".xml");
            if (applicationsXml != null)
            {
                XmlDocument document = new();
                document.Load(applicationsXml.OpenRead());
                xmlNavigator = document.CreateNavigator();
            }

            foreach (var type in types)
            {
                var item = type.GetCustomAttributes<FrontEndRoutingAttribute>().First();
                item.Code ??= type.Name;
                list.Add(item);
                FunctionInfosCache.TryAdd(item.Code, GetFunctions(type, xmlNavigator, item.Code));
            }

            // 加入缓存
            FrontEndRoutingAttributesCache.TryAdd(assemblyName!, list);
        }

        return list;
    }

    /// <summary>
    /// 读取权限菜单资源
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="codes"></param>
    /// <param name="appId">应用ID</param>
    /// <returns></returns>
    public static List<MenuTreeInfo> GetPermissionResources(
        Assembly assembly,
        IList<string> codes,
        string appId
    )
    {
        var attributes = GetFrontEndRoutingAttributes(assembly);
        // 根据模块分组
        var moduleGroups = attributes
            // 排除掉上级Code不为空的，因为上级Code不为空的是子菜单
            .Where(p => p.ParentCode == null)
            .OrderBy(p => p.ModuleSort)
            .GroupBy(p => p.ModuleCode);

        // 读取有上级Code的数据
        var children = attributes
            // 读取包含在权限中的
            .Where(p =>
                // 包含就是已授权
                codes.Contains(p.Code ?? p.Name) ||
                // 或者不需要授权的
                p.IsAuth == false
            )
            .Where(p => p.ParentCode != null)
            .OrderBy(p => p.Sort)
            .Select((p, index) => new MenuTreeInfo
            {
                AppId = appId,
                ParentCode = p.ParentCode,
                Name = p.Name,
                Path = p.RoutePath,
                Icon = p.Icon,
                IsVisible = p.IsVisible,
                IsAuth = p.IsAuth,
                Code = p.Code ?? p.Name,
                Description = p.Description,
                Sort = index,
                Functions = FunctionInfosCache
                    .GetValueOrDefault(p.Code ?? p.Name)?
                    .Where(c => codes.Contains(c.Key))
                    .ToList()
            })
            .ToList();
        // 
        var result = moduleGroups
            .Select((group, mIndex) => new MenuTreeInfo
            {
                AppId = appId,
                Code = group.Key,
                Path = group.First().ModuleRoutePath,
                Icon = group.First().ModuleIcon,
                Name = group.First().ModuleName,
                Sort = mIndex,
                Children = group
                    // 读取包含在权限中的
                    .Where(p =>
                        // 包含就是已授权
                        codes.Contains(p.Code ?? p.Name) ||
                        // 或者不需要授权的
                        p.IsAuth == false
                    )
                    .OrderBy(p => p.Sort)
                    .Select((p, index) => new MenuTreeInfo
                    {
                        AppId = appId,
                        ParentCode = group.Key,
                        Name = p.Name,
                        Path = p.RoutePath,
                        Icon = p.Icon,
                        IsVisible = p.IsVisible,
                        IsAuth = p.IsAuth,
                        Code = p.Code ?? p.Name,
                        Description = p.Description,
                        Sort = index,
                        Functions = FunctionInfosCache.GetValueOrDefault(p.Code ?? p.Name)?
                            .Where(c => codes.Contains(c.Key))
                            .ToList(),
                        Children = GetMenuTreeInfos(children, p.Code ?? p.Name)
                    })
                    .ToList()
            })
            .ToList();

        // 如果没有权限的则不显示
        result = result
            .Where(p => p.Children != null && p.Children.Any())
            .ToList();

        return result;
    }

    /// <summary>
    /// 读取权限菜单资源
    /// </summary>
    /// <param name="list"></param>
    /// <param name="codes"></param>
    /// <param name="appId">应用ID</param>
    /// <returns></returns>
    public static List<MenuTreeInfo> GetPermissionResources(
        IList<MenuTreeInfo> list,
        IList<string> codes,
        string appId
    )
    {
        // 递归处理list，只保留codes中有的数据
        var result = list
            .OrderBy(p => p.Sort)
            .Select((p, index) => new MenuTreeInfo
            {
                AppId = appId,
                ParentCode = p.ParentCode,
                Name = p.Name,
                Path = p.Path,
                Icon = p.Icon,
                IsVisible = p.IsVisible,
                IsAuth = p.IsAuth,
                Code = p.Code,
                Description = p.Description,
                Sort = index,
                Functions = p.Functions?
                    .Where(c => codes.Contains(c.Key))
                    .ToList(),
                Children = p.Children == null || p.Children.Count == 0
                    ? null
                    : GetPermissionResources(
                        p.Children
                            // 包含就是已授权
                            .Where(x => codes.Contains(x.Code))
                            .ToList(),
                        codes,
                        appId
                    )
            })
            .ToList();
        return result;
    }

    /// <summary>
    /// 递归处理菜单，用ParentCode来关联
    /// </summary>
    /// <param name="list"></param>
    /// <param name="parentCode"></param>
    /// <returns></returns>
    private static List<MenuTreeInfo>? GetMenuTreeInfos(IList<MenuTreeInfo> list, string parentCode)
    {
        var result = list
            .Where(p => p.ParentCode == parentCode)
            .OrderBy(p => p.Sort)
            .ToList();
        foreach (var item in result)
        {
            item.Children = GetMenuTreeInfos(list, item.Code);
        }

        return result.Count == 0 ? null : result;
    }

    /// <summary>
    /// 读取菜单资源
    /// </summary>
    /// <param name="assembly">程序集</param>
    /// <param name="appId">应用ID</param>
    /// <returns></returns>
    public static List<MenuTreeInfo> GetResources(Assembly assembly, string appId)
    {
        var list = GetFrontEndRoutingAttributes(assembly);
        return GetResources(list, appId);
    }

    /// <summary>
    /// 读取菜单资源
    /// </summary>
    /// <param name="attributes"></param>
    /// <param name="appId">应用ID</param>
    /// <returns></returns>
    public static List<MenuTreeInfo> GetResources(IList<FrontEndRoutingAttribute> attributes, string appId)
    {
        // 根据模块分组
        var moduleGroups = attributes
            // 排除掉上级Code不为空的，因为上级Code不为空的是子菜单
            .Where(p => p.ParentCode == null)
            .OrderBy(p => p.ModuleSort)
            .GroupBy(p => p.ModuleCode);

        // 读取有上级Code的数据
        var children = attributes
            .Where(p => p.ParentCode != null)
            .OrderBy(p => p.Sort)
            .Select((p, index) => new MenuTreeInfo
            {
                AppId = appId,
                ParentCode = p.ParentCode,
                Name = p.Name,
                Path = p.RoutePath,
                Icon = p.Icon,
                IsVisible = p.IsVisible,
                IsAuth = p.IsAuth,
                Code = p.Code ?? p.Name,
                Description = p.Description,
                Sort = index,
                Functions = FunctionInfosCache
                    .GetValueOrDefault(p.Code ?? p.Name)?
                    .ToList()
            })
            .ToList();
        // 
        var result = moduleGroups
            .Select((group, mIndex) => new MenuTreeInfo
            {
                AppId = appId,
                Code = group.Key,
                Path = group.First().ModuleRoutePath,
                Icon = group.First().ModuleIcon,
                Name = group.First().ModuleName,
                Sort = mIndex,
                Children = group
                    .OrderBy(p => p.Sort)
                    .Select((p, index) => new MenuTreeInfo
                    {
                        AppId = appId,
                        ParentCode = group.Key,
                        Name = p.Name,
                        Path = p.RoutePath,
                        Icon = p.Icon,
                        IsVisible = p.IsVisible,
                        IsAuth = p.IsAuth,
                        Code = p.Code ?? p.Name,
                        Description = p.Description,
                        Sort = index,
                        Functions = FunctionInfosCache.GetValueOrDefault(p.Code ?? p.Name)?
                            .ToList(),
                        Children = GetMenuTreeInfos(children, p.Code ?? p.Name)
                    })
                    .ToList()
            })
            .ToList();
        return result;
    }

    /// <summary>
    /// 读取资源代码信息
    /// </summary>
    /// <param name="assembly">程序集</param>
    /// <param name="appId"></param>
    /// <returns></returns>
    public static List<ResourceCodeInfo> GetResourceCodeInfos(Assembly assembly, string appId)
    {
        var types = assembly.GetExportedTypes()
            .Where(p => p.GetCustomAttributes()
                .Any(c => c.GetType() == typeof(FrontEndRoutingAttribute))
            )
            .ToList();

        var codes = new List<ResourceCodeInfo>();

        // 根据模块分组
        var moduleGroups = types
            .GroupBy(p => p.GetCustomAttribute<FrontEndRoutingAttribute>()!.ModuleCode);

        foreach (var group in moduleGroups)
        {
            codes.Add(new ResourceCodeInfo
            {
                ApplicationId = appId,
                Key = group.Key,
                Code = group.Key
            });
            foreach (var type in group)
            {
                codes.Add(new ResourceCodeInfo
                {
                    ApplicationId = appId,
                    Key = type.GetCustomAttribute<FrontEndRoutingAttribute>()!.Code ?? type.Name,
                    Code = type.GetCustomAttribute<FrontEndRoutingAttribute>()!.Code ?? type.Name
                });
                codes.AddRange(GetFunctionCodeInfos(type));
            }
        }

        return codes;
    }

    /// <summary>
    /// 读取功能代码
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static IEnumerable<ResourceCodeInfo> GetFunctionCodeInfos(Type type)
    {
        List<ResourceCodeInfo> functionCodeInfos = new();
        var methodInfos = type.GetMethods()
            // 只读取公开的方法
            .Where(p => p.IsPublic)
            // 非父类的方法
            .Where(p => p.DeclaringType == type)
            .ToList();
        foreach (var info in methodInfos)
        {
            var value = $"{type.Name}_{info.Name}";
            var key = value;

            // 如果有自定义的权限特性
            if (info.GetCustomAttribute(typeof(ApiPermissionAttribute)) is ApiPermissionAttribute permissionInfo)
            {
                // 不包含自身的跳过
                if (!permissionInfo.IsVisible)
                {
                    continue;
                }

                // 别名
                if (!string.IsNullOrEmpty(permissionInfo.Alias))
                {
                    value = permissionInfo.Alias;
                    key = permissionInfo.Alias;
                }

                // 如果有附加规则
                if (permissionInfo.AdditionalRules is {Length: > 0})
                {
                    var otherValues = string.Join(",", permissionInfo.AdditionalRules);
                    value = $"{value},{otherValues}";
                }
            }

            functionCodeInfos.Add(new ResourceCodeInfo
            {
                Key = key,
                Code = value
            });
        }

        return functionCodeInfos;
    }

    /// <summary>
    /// 读取功能
    /// </summary>
    /// <param name="type"></param>
    /// <param name="xmlNavigator"></param>
    /// <param name="parentCode"></param>
    /// <returns></returns>
    private static List<FunctionInfo> GetFunctions(Type type, XPathNavigator? xmlNavigator, string? parentCode)
    {
        List<FunctionInfo> functionInfos = new();
        var methodInfos = type.GetMethods()
            // 只读取公开的方法
            .Where(p => p.IsPublic)
            // 非父类的方法
            .Where(p => p.DeclaringType == type)
            .ToList();
        foreach (var info in methodInfos)
        {
            // 如果有AllowAnonymous特性跳过
            if (info.GetCustomAttribute(typeof(AllowAnonymousAttribute)) is not null)
            {
                continue;
            }

            var value = $"{type.Name}_{info.Name}";
            var key = value;
            var label = GetMethodSummaryName(xmlNavigator, info, info.Name);
            string? desc = null;
            // 如果有自定义的权限特性
            if (info.GetCustomAttribute(typeof(ApiPermissionAttribute)) is ApiPermissionAttribute permissionInfo)
            {
                // 不包含自身的跳过
                if (!permissionInfo.IsVisible)
                {
                    continue;
                }

                // 别名
                if (!string.IsNullOrEmpty(permissionInfo.Alias))
                {
                    value = permissionInfo.Alias;
                    key = permissionInfo.Alias;
                }

                // 如果有附加规则
                if (permissionInfo.AdditionalRules is {Length: > 0})
                {
                    var otherValues = string.Join(",", permissionInfo.AdditionalRules);
                    value = $"{value},{otherValues}";
                }

                // 显示名称
                if (!string.IsNullOrEmpty(permissionInfo.DisplayName))
                {
                    label = permissionInfo.DisplayName;
                }

                // 描述
                if (!string.IsNullOrEmpty(permissionInfo.Description))
                {
                    desc = permissionInfo.Description;
                }
            }

            var treeInfo = new FunctionInfo
            {
                Key = key,
                Value = value,
                Label = label,
                Description = desc,
                ParentCode = parentCode
            };
            functionInfos.Add(treeInfo);
        }

        return functionInfos;
    }

    /// <summary>
    /// 读取方法注释
    /// </summary>
    /// <param name="xmlNavigator"></param>
    /// <param name="methodInfo"></param>
    /// <param name="defaultName"></param>
    /// <returns></returns>
    private static string GetMethodSummaryName(
        XPathNavigator? xmlNavigator,
        MethodInfo methodInfo,
        string defaultName)
    {
        if (xmlNavigator == null)
        {
            return defaultName;
        }

        var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo);
        var propertySummaryNode =
            xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']/summary");
        return propertySummaryNode == null
            ? defaultName
            : $"{XmlCommentsTextHelper.Humanize(propertySummaryNode.InnerXml)}";
    }
}