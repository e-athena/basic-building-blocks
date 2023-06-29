using System.Xml;

namespace Athena.Infrastructure.Summaries;

/// <summary>
/// 
/// </summary>
public static class EntitySummaryHelper
{
    /// <summary>
    /// 获取实体注释
    /// </summary>
    /// <returns></returns>
    public static EntitySummary? GetEntitySummary(Type type, params string[] assembly)
    {
        return GetEntitySummaries(new List<Type> {type}, assembly)?.FirstOrDefault();
    }

    /// <summary>
    /// 获取实体注释
    /// </summary>
    /// <returns></returns>
    public static EntitySummary? GetEntitySummary<T>(params string[] assembly)
    {
        return GetEntitySummary(typeof(T), assembly);
    }

    /// <summary>
    /// 获取实体注释
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<EntitySummary>? GetEntitySummaries(IEnumerable<Type> types, params string[] assembly)
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
            return new List<EntitySummary>();
        }

        XmlDocument document = new();
        document.Load(applicationsXml.OpenRead());
        var xmlNavigator = document.CreateNavigator();
        if (xmlNavigator == null)
        {
            return new List<EntitySummary>();
        }

        List<EntitySummary> summaryList = new();
        foreach (var item in types)
        {
            var memberName = XmlCommentsNodeNameHelper.GetMemberNameForType(item);
            var summaryNode = xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{memberName}']/summary");
            var sumName = item.Name;
            if (summaryNode != null)
            {
                sumName = summaryNode.InnerXml.Replace("\n", "").Replace(" ", "");
            }

            EntitySummary summary = new()
            {
                Name = sumName,
                Key = item.Name,
                Items = new List<EntitySummary>()
            };
            foreach (var info in item.GetProperties())
            {
                var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(info);
                var propertySummaryNode =
                    xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']/summary");
                summary.Items.Add(new EntitySummary
                {
                    Key = info.Name,
                    Name = propertySummaryNode == null
                        ? info.Name
                        : XmlCommentsTextHelper.Humanize(propertySummaryNode.InnerXml)
                });
            }

            summaryList.Add(summary);
        }

        return summaryList.Count == 0 ? null : summaryList;
    }

    /// <summary>
    /// 获取实体注释
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<EntitySummary> GetPermissionSummaryByMethods(IEnumerable<Type> types,
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
            return new List<EntitySummary>();
        }

        XmlDocument document = new();
        document.Load(applicationsXml.OpenRead());
        var xmlNavigator = document.CreateNavigator();
        if (xmlNavigator == null)
        {
            return new List<EntitySummary>();
        }

        List<EntitySummary> summaryList = new();
        foreach (var item in types)
        {
            var memberName = XmlCommentsNodeNameHelper.GetMemberNameForType(item);
            var summaryNode = xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{memberName}']/summary");
            var sumName = item.Name;
            if (summaryNode != null)
            {
                sumName = summaryNode.InnerXml.Replace("\n", "").Replace(" ", "");
            }

            EntitySummary summary = new()
            {
                Name = string.IsNullOrEmpty(sumName) ? item.Name : sumName,
                Key = item.Name,
                Items = new List<EntitySummary>()
            };
            var methodInfos = item.GetMethods()
                .Where(p => p.GetCustomAttributes()
                    .Any(c => c.GetType() == typeof(PermissionAttribute)))
                .ToList();
            foreach (var info in methodInfos)
            {
                var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(info);
                var propertySummaryNode =
                    xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']/summary");
                summary.Items.Add(new EntitySummary
                {
                    Key = $"{item.Name}_{info.Name}",
                    Name = propertySummaryNode == null
                        ? $"[{summary.Name}]{info.Name}"
                        : $"[{summary.Name}]{XmlCommentsTextHelper.Humanize(propertySummaryNode.InnerXml)}"
                });
            }

            summaryList.Add(summary);
        }

        return summaryList;
    }
}