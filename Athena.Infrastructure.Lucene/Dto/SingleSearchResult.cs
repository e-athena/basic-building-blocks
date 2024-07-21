using Athena.Infrastructure.Lucene.Interfaces;

namespace Athena.Infrastructure.Lucene.Dto;

public class SingleSearchResult : ISearchResult<SearchResultItem>
{
    /// <summary>
    /// 匹配结果
    /// </summary>
    public IList<SearchResultItem> Items { get; set; }
    /// <summary>
    /// 检索耗时
    /// </summary>
    public long Elapsed { get; set; }
    /// <summary>
    /// 匹配结果数
    /// </summary>
    public int TotalHits { get; set; }

    public SingleSearchResult()
    {
        Items = new List<SearchResultItem>();
    }
}