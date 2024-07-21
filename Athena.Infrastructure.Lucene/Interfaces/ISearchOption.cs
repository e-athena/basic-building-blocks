namespace Athena.Infrastructure.Lucene.Interfaces;

public interface ISearchOption
{
    /// <summary>
    /// 最大检索量
    /// </summary>
    int MaxHits { get; set; }
}