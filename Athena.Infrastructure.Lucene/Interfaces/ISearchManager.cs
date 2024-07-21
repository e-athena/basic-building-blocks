using Athena.Infrastructure.Lucene.Dto;
using Lucene.Net.Analysis;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;

namespace Athena.Infrastructure.Lucene.Interfaces;

/// <summary>
/// 搜索管理器
/// </summary>
/// <typeparam name="TAnalyzer"></typeparam>
/// <typeparam name="TDirectory"></typeparam>
/// <typeparam name="TTaxoDirectory"></typeparam>
public interface ISearchManager<out TAnalyzer, out TDirectory, out TTaxoDirectory> where TAnalyzer : Analyzer
    where TDirectory : Directory
    where TTaxoDirectory : FSDirectory
{
    /// <summary>
    /// 分析器
    /// </summary>
    TAnalyzer Analyzer { get; }

    /// <summary>
    /// 普通索引存储路径
    /// </summary>
    TDirectory Directory { get; }

    /// <summary>
    /// 维度索引存储路径
    /// </summary>
    TTaxoDirectory TaxoDirectory { get; }


    /// <summary>
    /// 构建query
    /// </summary>
    /// <param name="queryList"></param>
    /// <returns></returns>
    Query BuildQuery(List<QuerySearchDto> queryList);


    /// <summary>
    /// 创建索引
    /// </summary>
    /// <param name="fields"></param>
    void CreateIndex(Dictionary<string, object> fields);

    /// <summary>
    /// 批量创建索引
    /// </summary>
    /// <param name="fields">Field域集合，也可以是其他结构</param>
    /// <param name="uniKey"></param>
    void CreateIndex(List<Dictionary<string, object>> fields, string uniKey = "id");

    /// <summary>
    /// 批量创建索引
    /// </summary>
    /// <param name="fields">Field域集合，也可以是其他结构</param>
    /// <param name="version"></param>
    /// <param name="uniKey">主键</param>
    void CreateIndex(List<Dictionary<string, object>> fields, string version, string uniKey = "id");

    /// <summary>
    /// 删除索引
    /// </summary>
    /// <param name="docId">文档</param>
    void DeleteIndex(int docId);

    /// <summary>
    /// 删除索引
    /// </summary>
    /// <param name="query">文档</param>
    void DeleteIndex(Query query);

    /// <summary>
    /// 删除所有索引
    /// </summary>
    void DeleteAllIndex();

    /// <summary>
    /// 更新索引
    /// </summary>
    /// <param name="doc">文档</param>
    /// <param name="dic">要更新的域</param>
    void UpdateIndex(Document doc, Dictionary<string, string> dic);

    /// <summary>
    /// 索引计数
    /// </summary>
    /// <returns></returns>
    int CountIndex();

    /// <summary>
    /// 关键词查询
    /// </summary>
    /// <param name="keyword"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    Dictionary<Document, float> SearchIndex(string keyword, string field);

    /// <summary>
    /// Query对象查询  自定义查询条件
    /// </summary>
    /// <param name="query">关键词</param>
    /// <returns></returns>
    List<SearchDocDto> SearchIndex(Query query);

    /// <summary>
    /// 为实体对象创建索引
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="isFiltered">是否启用属性过滤，默认开启</param>
    /// <param name="isCreate">新增或更新，默认新增</param>
    void CreateIndexByEntity(IEntity<string> entity, bool isFiltered = true, bool isCreate = true);


    /// <summary>
    /// 简单查询（多域）
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    SingleSearchResult SingleSearch(SingleSearchOption option);

    /// <summary>
    /// 包含分页的查询
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    PagedSearchResult PagedSearch(PagedSearchOption option);

    /// <summary>
    /// 多分组查询
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    List<PagedSearchResult> PageSearchList(List<PagedSearchOption> options);

    /// <summary>
    /// 包含权重的查询
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    ScoredSearchResult ScoredSearch(ScoredSearchOption option);

    /// <summary>
    /// 计数查询
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    List<CountSearchResultItem> CountSearch(CountSearchOption option);

    /// <summary>
    /// 维度查询
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    FacetSearchResult FacetSearch(FacetSearchOption option);

    /// <summary>
    /// 分组查询
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    GroupSearchResult GroupSearch(GroupSearchOption option);

    /// <summary>
    /// 地理信息查询
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    GeoSearchResult GeoSearch(GeoSearchOption option);

    /// <summary>
    /// 索引基本信息
    /// </summary>
    /// <returns></returns>
    IndexInfo Info();


    /// <summary>
    /// 检查击中数量
    /// </summary>
    /// <param name="queryList"></param>
    /// <returns></returns>
    List<QuerySearchDto> CheckHitCount(List<QuerySearchDto> queryList);
}

public interface ISearchManager : ISearchManager<Analyzer, Directory, FSDirectory>
{
}