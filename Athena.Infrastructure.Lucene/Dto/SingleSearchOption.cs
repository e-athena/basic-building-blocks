namespace Athena.Infrastructure.Lucene.Dto;

public class SingleSearchOption:SearchOptionBase
{
    /// <summary>
    /// 检索关键词
    /// </summary>
    public string Keyword { get; set; }

    /// <summary>
    /// 限定检索域
    /// </summary>
    public List<string> Fields { get; set; }

    /// <summary>
    /// 是否高亮显示
    /// </summary>
    public bool IsHightLight { get; set; }

    public SingleSearchOption(string keyword,List<string> fields,int maxHits=100,bool isHightLight=false)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            throw new ArgumentException("搜索关键词不能为空");
        }
        Keyword = keyword;
        Fields = fields;
        MaxHits = maxHits;
        IsHightLight = isHightLight;
    }

    public SingleSearchOption()
    {

    }

}