namespace Athena.Infrastructure.Lucene.Dto;

public class GeoSearchResult
{
    /// <summary>
    /// 检索耗时
    /// </summary>
    public long Elapsed { get; set; }
    /// <summary>
    /// 匹配结果数
    /// </summary>
    public int TotalHits { get; set; }

    public Dictionary<Document,float> Items { get; set; }

    public GeoSearchResult()
    {
        Items = new Dictionary<Document, float>();
    }
}