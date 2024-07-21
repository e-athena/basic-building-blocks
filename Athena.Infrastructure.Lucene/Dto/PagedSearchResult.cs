namespace Athena.Infrastructure.Lucene.Dto;

public class PagedSearchResult
{
    public List<PagedSearchResultItem> Items { get; set; }
    public long Elapsed { get; set; }
    public int TotalHits { get; set; }

    /// <summary>
    /// 查询条件
    /// </summary>
    public PagedSearchOption SearchOption { get; set; }

    /// <summary>
    ///
    /// </summary>
    public PagedSearchResult()
    {
        Items = new List<PagedSearchResultItem>();
    }
}