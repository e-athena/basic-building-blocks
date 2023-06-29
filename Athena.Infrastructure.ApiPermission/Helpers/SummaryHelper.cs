namespace Athena.Infrastructure.ApiPermission.Helpers;

/// <summary>
/// SummaryHelper
/// </summary>
public static class SummaryHelper
{
    /// <summary>
    /// 获取实体注释
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<ResourceTreeInfo> GetSummaryByMethods(IEnumerable<Type> types,
        params string[] assembly)
    {
        var path = AppDomain.CurrentDomain.BaseDirectory;
        var files = new DirectoryInfo(path).GetFiles();
        // 找出里面的xml
        var applicationsXml = files
            .Where(n => !string.IsNullOrEmpty(n.Extension))
            .Where(n => assembly.Contains(n.Name.Replace(n.Extension, "")))
            .FirstOrDefault(p => p.Extension.ToLower() == ".xml");
        if (applicationsXml == null)
        {
            return new List<ResourceTreeInfo>();
        }

        XmlDocument document = new();
        document.Load(applicationsXml.OpenRead());
        var xmlNavigator = document.CreateNavigator();
        if (xmlNavigator == null)
        {
            return new List<ResourceTreeInfo>();
        }

        List<ResourceTreeInfo> summaryList = new();
        foreach (var item in types)
        {
            var memberName = XmlCommentsNodeNameHelper.GetMemberNameForType(item);
            var summaryNode = xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{memberName}']/summary");
            var sumName = item.Name;
            if (summaryNode != null)
            {
                sumName = summaryNode.InnerXml.Replace("\n", "").Replace(" ", "");
            }

            ResourceTreeInfo summary = new()
            {
                Label = string.IsNullOrEmpty(sumName) ? item.Name : sumName,
                Key = item.Name,
                Children = new List<ResourceTreeInfo>()
            };

            var methodInfos = item.GetMethods()
                .Where(p => p.GetCustomAttributes()
                    .Any(c => c.GetType() == typeof(ApiPermissionAttribute)))
                .ToList();
            foreach (var info in methodInfos)
            {
                var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(info);
                var propertySummaryNode =
                    xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']/summary");
                var permissionInfo = (ApiPermissionAttribute) info.GetCustomAttribute(typeof(ApiPermissionAttribute))!;
                // 不包含自身的跳过
                if (!permissionInfo.IsVisible)
                {
                    continue;
                }

                var value = $"{item.Name}_{info.Name}";
                var key = value;
                // 别名
                if (!string.IsNullOrEmpty(permissionInfo.Alias))
                {
                    value = permissionInfo.Alias;
                    key = permissionInfo.Alias;
                }

                if (permissionInfo.AdditionalRules is {Length: > 0})
                {
                    var otherValues = string.Join(",", permissionInfo.AdditionalRules);
                    value = $"{value},{otherValues}";
                }

                var label = propertySummaryNode == null
                    ? info.Name
                    : $"{XmlCommentsTextHelper.Humanize(propertySummaryNode.InnerXml)}";
                if (!string.IsNullOrEmpty(permissionInfo.DisplayName))
                {
                    label = permissionInfo.DisplayName;
                }

                var treeInfo = new ResourceTreeInfo
                {
                    Key = key,
                    Extend = $"{item.Name}_{info.Name}",
                    Value = value,
                    Label = label
                };
                summary.Children.Add(treeInfo);
            }

            summaryList.Add(summary);
        }

        return summaryList;
    }

    /// <summary>
    /// 获取实体注释
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<ResourceTreeInfo> GetSummaryByClasses(IEnumerable<Type> types,
        params string[] assembly)
    {
        var path = AppDomain.CurrentDomain.BaseDirectory;
        var files = new DirectoryInfo(path).GetFiles();
        // 找出里面的xml
        var applicationsXml = files
            .Where(n => !string.IsNullOrEmpty(n.Extension))
            .Where(n => assembly.Contains(n.Name.Replace(n.Extension, "")))
            .FirstOrDefault(p => p.Extension.ToLower() == ".xml");
        if (applicationsXml == null)
        {
            return new List<ResourceTreeInfo>();
        }

        XmlDocument document = new();
        document.Load(applicationsXml.OpenRead());
        var xmlNavigator = document.CreateNavigator();
        if (xmlNavigator == null)
        {
            return new List<ResourceTreeInfo>();
        }

        List<ResourceTreeInfo> summaryList = new();
        foreach (var item in types)
        {
            var memberName = XmlCommentsNodeNameHelper.GetMemberNameForType(item);
            var summaryNode = xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{memberName}']/summary");
            var sumName = item.Name;
            if (summaryNode != null)
            {
                sumName = summaryNode.InnerXml.Replace("\n", "").Replace(" ", "");
            }

            ResourceTreeInfo summary = new()
            {
                Label = string.IsNullOrEmpty(sumName) ? item.Name : sumName,
                Key = item.Name,
                Children = new List<ResourceTreeInfo>()
            };

            var methodInfos = item.GetMethods()
                // 只读取公开的方法
                .Where(p => p.IsPublic)
                // 非父类的方法
                .Where(p => p.DeclaringType == item)
                .ToList();
            foreach (var info in methodInfos)
            {
                var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(info);
                var propertySummaryNode =
                    xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']/summary");
                var value = $"{item.Name}_{info.Name}";
                var key = value;
                var label = propertySummaryNode == null
                    ? info.Name
                    : $"{XmlCommentsTextHelper.Humanize(propertySummaryNode.InnerXml)}";

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

                    // 如果有自定义的显示名称
                    if (!string.IsNullOrEmpty(permissionInfo.DisplayName))
                    {
                        label = permissionInfo.DisplayName;
                    }
                }

                var treeInfo = new ResourceTreeInfo
                {
                    Key = key,
                    Extend = $"{item.Name}_{info.Name}",
                    Value = value,
                    Label = label
                };
                summary.Children.Add(treeInfo);
            }

            summaryList.Add(summary);
        }

        return summaryList;
    }
}