namespace Athena.Infrastructure.Lucene.Dto;

/// <summary>
///
/// </summary>
public class PagedSearchOption : SearchOptionBase
{
    public int PageSize { get; set; }
    public int PageIndex { get; set; }

    public List<SortByModel> Sorts { get; set; }

    public List<QuerySearchDto> QueryList { get; set; }

    /// <summary>
    /// 匹配度，0-1，数值越大结果越精确
    /// </summary>
    public float Score { get; set; } = 0.1f;

    public PagedSearchOption(List<QuerySearchDto> queryList, List<SortByModel> sorts, int maxHits = 1000,
        int pageSize = 10, int pageIndex = 1)
    {
        if (queryList.Count == 0)
        {
            throw new ArgumentException("搜索关键词不能为空");
        }

        QueryList = queryList;
        Sorts = sorts;
        MaxHits = maxHits;
        if (pageSize < 1)
        {
            pageSize = 1;
        }

        if (pageIndex < 1)
        {
            pageIndex = 1;
        }

        PageSize = pageSize;
        PageIndex = pageIndex;
    }
}