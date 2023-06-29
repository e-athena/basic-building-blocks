namespace Athena.Infrastructure.Summaries;

/// <summary>
/// 
/// </summary>
public class EntitySummary
{
    /// <summary>
    /// Key
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 子项
    /// </summary>
    public List<EntitySummary>? Items { get; set; }
}