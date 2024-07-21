using Athena.Infrastructure.Lucene.Interfaces;

namespace Athena.Infrastructure.Lucene.Dto;

public class SearchOptionBase : ISearchOption
{
    /// <summary>
    /// 最大检索量
    /// </summary>
    public int MaxHits { get ; set; }
}