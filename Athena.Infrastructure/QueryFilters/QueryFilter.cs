namespace Athena.Infrastructure.QueryFilters;

/// <summary>
/// 自定义查询过滤器
/// </summary>
public class QueryFilter
{
    /// <summary>
    /// Key
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Value
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// 运算符
    /// </summary>
    public string Operator { get; set; } = null!;

    /// <summary>
    /// 与或，or or and
    /// </summary>
    public string XOR { get; set; } = null!;

    /// <summary>
    /// 属性类型
    /// </summary>
    public string PropertyType { get; set; } = "string";

    /// <summary>
    /// 扩展方法名
    /// </summary>
    /// <value></value>
    public string? ExtendFuncMethodName { get; set; }
}