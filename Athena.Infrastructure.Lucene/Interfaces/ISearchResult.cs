namespace Athena.Infrastructure.Lucene.Interfaces;

public interface ISearchResult : ISearchResult<ISearchResultItem>
{
}

public interface ISearchResult<T>
{
    IList<T> Items { get; set; }
    long Elapsed { get; set; }
    int TotalHits { get; set; }
}