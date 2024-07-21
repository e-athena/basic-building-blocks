using Athena.Infrastructure.Lucene.Interfaces;

namespace Athena.Infrastructure.Lucene.Dto;

public class ScoredSearchResult : ISearchResult<SearchResultItem>
{
    public IList<SearchResultItem> Items { get; set; }
    public long Elapsed { get;set;}
    public int TotalHits { get; set; }

    public ScoredSearchResult()
    {
        Items = new List<SearchResultItem>();
    }
}