using Athena.Infrastructure.Lucene.Interfaces;

namespace Athena.Infrastructure.Lucene.Dto;

public class SearchResultItem : ISearchResultItem
{
    /// <summary>
    /// 结果评分
    /// </summary>
    public float Score { get; set; }
    /// <summary>
    /// 实体Id
    /// </summary>
    public string EntityId { get; set; }
    /// <summary>
    /// 实体类名
    /// </summary>
    public string EntityName { get; set; }

    /// <summary>
    /// 结果值
    /// </summary>
    public string FieldValue { get; set; }

}