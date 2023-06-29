using Athena.Infrastructure.QueryFilters;

namespace Athena.Infrastructure.Messaging.Requests;

/// <summary>
/// 请求基类
/// </summary>
public class GetRequestBase
{
    /// <summary>
    /// 关键字
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 排序值
    /// </summary>
    public virtual string? Sorter { get; set; }

    /// <summary>
    /// 查询过滤器组
    /// </summary>
    public IList<QueryFilterGroup>? FilterGroups { get; set; }
}