using Athena.Infrastructure.Lucene.Interfaces;

namespace Athena.Infrastructure.Lucene.Dto;

public class PagedSearchResultItem : ISearchResultItem
{
    public float Score { get; set; }

    public Document Doc { get; set; }
}