namespace Athena.Infrastructure.Lucene;

/// <summary>
///
/// </summary>
public class SearchManagerConfig
{
    /// <summary>
    /// 默认索引存储路径
    /// </summary>
    public virtual required string DefaultPath { get; set; }

    /// <summary>
    /// 维度索引存储路径
    /// </summary>
    public virtual required string FacetPath { get; set; }

    /// <summary>
    /// 词典路径
    /// </summary>
    public virtual required string DictPath { get; set; }
}