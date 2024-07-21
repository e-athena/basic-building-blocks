namespace Athena.Infrastructure.Lucene.Dto;

public class ScoredSearchOption:SearchOptionBase
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
    /// 多字段搜索时，给字段设定搜索权重
    /// </summary>
    private readonly Dictionary<string, float> _boosts;

    /// <summary>
    /// 多字段搜索时，给字段设定搜索权重
    /// </summary>
    internal Dictionary<string, float> Boosts
    {
        get
        {
            foreach (var field in Fields.Where(field => _boosts.All(x => x.Key.ToUpper() != field.ToUpper())))
            {
                _boosts.Add(field, 2.0f);
            }

            return _boosts;
        }
    }

    /// <summary>
    /// 匹配度，0-1，数值越大结果越精确
    /// </summary>
    public float Score { get; set; } = 0.5f;

    /// <summary>
    /// 过滤条件
    /// </summary>
    public Filter Filter { get; set; }

    public ScoredSearchOption(string keyword,List<string> fields,int maxHits=100,Dictionary<string,float> boosts=null)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            throw new ArgumentException("搜索关键词不能为空");
        }

        Keyword = keyword;
        Fields = fields;
        MaxHits = maxHits;
        _boosts = boosts ?? new Dictionary<string, float>();
    }

    /// <summary>
    /// 设置权重
    /// </summary>
    /// <param name="field"></param>
    /// <param name="boost"></param>
    public void SetBoosts(string field,float boost)
    {
        _boosts[field] = boost;
    }

}