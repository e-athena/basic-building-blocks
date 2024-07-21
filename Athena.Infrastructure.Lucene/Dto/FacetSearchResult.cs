namespace Athena.Infrastructure.Lucene.Dto;

/// <summary>
///
/// </summary>
public class FacetSearchResult
{
    public virtual IList<FacetResult> Items { get; set; }

    public FacetSearchResult()
    {
        Items = new List<FacetResult>();
    }
}