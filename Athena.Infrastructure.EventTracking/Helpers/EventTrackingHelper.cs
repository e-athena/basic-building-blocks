using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using Athena.Infrastructure.Event;
using Athena.Infrastructure.Summaries;
using Athena.Infrastructure.ViewModels;
using DotNetCore.CAP;
using MediatR;

namespace Athena.Infrastructure.EventTracking.Helpers;

/// <summary>
/// 事件追踪帮助类
/// </summary>
public static class EventTrackingHelper
{
    /// <summary>
    /// 缓存
    /// </summary>
    private static readonly ConcurrentDictionary<string, IList<EventTrackingInfo>>
        EventTrackingListCache = new();

    /// <summary>
    /// 缓存
    /// </summary>
    private static readonly ConcurrentDictionary<string, IList<SelectViewModel>>
        EventSelectListCache = new();

    /// <summary>
    /// 读取事件跟踪信息列表
    /// </summary>
    /// <param name="assemblies">程序集</param>
    /// <returns></returns>
    public static IList<EventTrackingInfo> GetEventTrackingInfos(List<Assembly> assemblies)
    {
        var list = new List<EventTrackingInfo>();
        foreach (var assembly in assemblies)
        {
            var assemblyName = assembly.GetName().Name;
            // 读取缓存
            if (EventTrackingListCache.TryGetValue(assemblyName!, out var cacheResult))
            {
                list.AddRange(cacheResult);
            }
            else
            {
                // 读取带有EventTrackingAttribute的方法
                var types = assembly
                    .GetExportedTypes()
                    .Where(p => p.GetInterfaces()
                                    .Where(x => x.IsGenericType)
                                    .Any(x =>
                                        x.GetGenericTypeDefinition() == typeof(INotificationHandler<>)
                                    )
                                || p.GetInterfaces()
                                    .Any(x => x == typeof(ICapSubscribe))
                    )
                    .ToList();

                XPathNavigator? xmlNavigator = null;
                var filePath = assembly.ManifestModule.FullyQualifiedName.Replace(".dll", ".xml");
                if (File.Exists(filePath))
                {
                    XmlDocument document = new();
                    document.Load(new FileInfo(filePath).OpenRead());
                    xmlNavigator = document.CreateNavigator();
                }

                foreach (var type in types)
                {
                    // 事件类型，实现ICapSubscribe的为集成事件，实现INotificationHandler<>的为领域事件
                    var isDomainEvent = type.GetInterfaces()
                        .Where(x => x.IsGenericType)
                        .Any(x => x.GetGenericTypeDefinition() == typeof(INotificationHandler<>));

                    var methodInfos = type.GetMethods()
                        // 只读取公开的方法
                        .Where(p => p.IsPublic)
                        // 非父类的方法
                        .Where(p => p.DeclaringType == type)
                        .Where(p => p.GetCustomAttributes().Any(t => t.GetType() == typeof(EventTrackingAttribute)))
                        .ToList();
                    if (methodInfos.Count == 0)
                    {
                        continue;
                    }

                    var processorName = GetTypeSummaryName(xmlNavigator, type, type.Name);

                    foreach (var info in methodInfos)
                    {
                        var label = GetMethodSummaryName(xmlNavigator, info, info.Name);
                        // 读取方法第一个参数
                        var parameterInfos = info.GetParameters();
                        var parameterInfo = parameterInfos.FirstOrDefault();
                        if (parameterInfo == null)
                        {
                            continue;
                        }

                        var select = new EventTrackingInfo
                        {
                            EventType = isDomainEvent ? EventType.DomainEvent : EventType.IntegrationEvent,
                            EventName = string.IsNullOrEmpty(label) ? info.Name : label,
                            EventTypeName = parameterInfo.ParameterType.Name,
                            EventTypeFullName = parameterInfo.ParameterType.FullName!,
                            ProcessorName = processorName,
                            ProcessorFullName = type.FullName!
                        };
                        list.Add(select);
                    }
                }

                // 加入缓存
                EventTrackingListCache.TryAdd(assemblyName!, list);
            }
        }

        // 根据EventTypeFullName和ProcessorFullName去重
        list = list.GroupBy(p => new {p.EventTypeFullName, p.ProcessorFullName})
            .Select(p => p.First())
            .ToList();
        return list;
    }

    /// <summary>
    /// 读取事件跟踪信息树形列表
    /// </summary>
    /// <param name="assemblies">程序集</param>
    /// <returns></returns>
    public static IList<EventTrackingTreeInfo> GetEventTrackingTreeInfos(List<Assembly> assemblies)
    {
        var list = GetEventTrackingInfos(assemblies);
        // 按EventTypeFullName来分组
        var groupList = list.GroupBy(p => p.EventTypeFullName).ToList();
        var treeList = new List<EventTrackingTreeInfo>();
        foreach (var group in groupList)
        {
            var tree = new EventTrackingTreeInfo
            {
                EventType = group.First().EventType,
                EventName = group.First().EventName,
                EventTypeName = group.First().EventTypeName,
                EventTypeFullName = group.First().EventTypeFullName,
                Children = new List<EventTrackingTreeInfo>()
            };
            foreach (var item in group)
            {
                tree.Children.Add(new EventTrackingTreeInfo
                {
                    EventType = item.EventType,
                    EventName = item.EventName,
                    EventTypeName = item.EventTypeName,
                    EventTypeFullName = item.EventTypeFullName,
                    ProcessorName = item.ProcessorName,
                    ProcessorFullName = item.ProcessorFullName,
                });
            }

            treeList.Add(tree);
        }

        return treeList;
    }

    /// <summary>
    /// 读取事件下拉列表
    /// </summary>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IList<SelectViewModel> GetEventSelectList(List<Assembly> assemblies)
    {
        // 继续了EventBase的类型
        var list = new List<SelectViewModel>();
        foreach (var assembly in assemblies)
        {
            var assemblyName = assembly.GetName().Name;
            // 读取缓存
            if (EventSelectListCache.TryGetValue(assemblyName!, out var cacheResult))
            {
                list.AddRange(cacheResult);
            }
            else
            {
                // 读取继续了EventBase的类型
                var types = assembly
                    .GetExportedTypes()
                    .Where(p => p.BaseType != null)
                    .Where(p =>
                        p.BaseType == typeof(EventBase) ||
                        (
                            p.BaseType!.IsGenericType &&
                            p.BaseType.GetGenericTypeDefinition() == typeof(DomainEvent<>)
                        )
                    )
                    .ToList();

                XPathNavigator? xmlNavigator = null;
                var filePath = assembly.ManifestModule.FullyQualifiedName.Replace(".dll", ".xml");
                if (File.Exists(filePath))
                {
                    XmlDocument document = new();
                    document.Load(new FileInfo(filePath).OpenRead());
                    xmlNavigator = document.CreateNavigator();
                }

                foreach (var type in types)
                {
                    var processorName = GetTypeSummaryName(xmlNavigator, type, type.Name);
                    var typeName = type.Name;

                    list.Add(new SelectViewModel
                    {
                        Label = processorName.Replace("事件", ""),
                        Value = typeName
                    });
                }

                // 加入缓存
                EventSelectListCache.TryAdd(assemblyName!, list);
            }
        }

        // 根据Value去重
        list = list.GroupBy(p => new {p.Value})
            .Select(p => p.First())
            .ToList();
        return list;
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

    /// <summary>
    /// 读取实体注释
    /// </summary>
    /// <param name="xmlNavigator"></param>
    /// <param name="type"></param>
    /// <param name="defaultName"></param>
    /// <returns></returns>
    private static string GetTypeSummaryName(
        XPathNavigator? xmlNavigator,
        Type type,
        string defaultName)
    {
        if (xmlNavigator == null)
        {
            return defaultName;
        }

        var memberName = XmlCommentsNodeNameHelper.GetMemberNameForType(type);
        var summaryNode = xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{memberName}']/summary");
        var sumName = type.Name;
        if (summaryNode != null)
        {
            sumName = summaryNode.InnerXml.Replace("\n", "").Replace(" ", "");
        }

        return sumName;
    }
}

/// <summary>
/// 事件追踪树
/// </summary>
public class EventTrackingTreeInfo : EventTrackingInfo
{
    /// <summary>
    /// 子节点
    /// </summary>
    public List<EventTrackingTreeInfo>? Children { get; set; }
}

/// <summary>
/// 事件追踪
/// </summary>
public class EventTrackingInfo
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public EventType EventType { get; set; }

    /// <summary>
    /// 事件名
    /// </summary>
    public string EventName { get; set; } = null!;

    /// <summary>
    /// 事件类型名
    /// </summary>
    public string EventTypeName { get; set; } = null!;

    /// <summary>
    /// 事件类型全名
    /// </summary>
    public string EventTypeFullName { get; set; } = null!;

    /// <summary>
    /// 处理器名
    /// </summary>
    public string? ProcessorName { get; set; }

    /// <summary>
    /// 处理器全名
    /// </summary>
    public string? ProcessorFullName { get; set; }
}