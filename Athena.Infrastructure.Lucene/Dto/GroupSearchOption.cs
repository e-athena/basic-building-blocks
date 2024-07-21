using Athena.Infrastructure.Lucene.Interfaces;

namespace Athena.Infrastructure.Lucene.Dto;

public class GroupSearchOption:ISearchOption
{
    public virtual List<string> Fields { get; set; }

    public virtual int MaxHits { get; set; }

    public GroupSearchOption()
    {
        Fields = new List<string>();
        MaxHits = 10;
    }

    public GroupSearchOption(List<string> fields, int maxHits = 10)
    {
        Fields = fields;
        MaxHits = maxHits;
    }
}