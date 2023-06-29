namespace Athena.Infrastructure.QueryFilters;

/// <summary>
/// 自定义查询过滤器组
/// </summary>
public class QueryFilterGroup
{
    /// <summary>
    /// 与或，or or and
    /// </summary>
    public string XOR { get; set; } = null!;

    /// <summary>
    /// 自定义查询过滤器列表
    /// </summary>
    public IList<QueryFilter> Filters { get; set; } = new List<QueryFilter>();
}