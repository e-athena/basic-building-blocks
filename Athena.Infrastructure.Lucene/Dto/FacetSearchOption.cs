namespace Athena.Infrastructure.Lucene.Dto;

/// <summary>
///
/// </summary>
public class FacetSearchOption
{
    public virtual List<string> Fields { get; set; }

    public virtual int MaxHits { get; set; }

    /// <summary>
    ///
    /// </summary>
    public FacetSearchOption()
    {
        Fields = new List<string>();
        MaxHits = 10;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="fields"></param>
    /// <param name="maxHits"></param>
    public FacetSearchOption(List<string> fields,int maxHits=10)
    {
        Fields = fields;
        MaxHits = maxHits;
    }
}