namespace Athena.Infrastructure.Lucene.Dto;

/// <summary>
///
/// </summary>
public class QuerySearchDto
{
    /// <summary>
    /// 关键字
    /// </summary>
    public string Keyword { get; set; }

    /// <summary>
    /// 字段
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    /// 是否是范围查询
    /// </summary>
    public Occur Occur { get; set; }

    /// <summary>
    /// 击中数
    /// </summary>
    public int HitCount { get; set; }

    /// <summary>
    ///
    /// </summary>
    public bool IsIntRange { get; set; } = false;
}