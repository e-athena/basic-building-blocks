using Athena.Infrastructure.Lucene.Interfaces;

namespace Athena.Infrastructure.Lucene.Dto;

public class GroupSearchResult:ISearchResult<string>
{
    public virtual IList<string> Items { get; set; }
    public virtual long Elapsed { get ; set; }
    public virtual int TotalHits { get ; set ; }

    public GroupSearchResult()
    {
        Items = new List<string>();
        Elapsed = 0;
        TotalHits = 0;
    }
}