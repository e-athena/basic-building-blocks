using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Athena.Infrastructure.Lucene.Dto;
using Athena.Infrastructure.Lucene.Interfaces;
using JiebaNet.Segmenter.Common;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Facet.Taxonomy.Directory;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search.Grouping;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Spatial;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Spatial.Queries;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Spatial4n.Core.Distance;
using Spatial4n.Core.Shapes;
using Directory = Lucene.Net.Store.Directory;

namespace Athena.Infrastructure.Lucene;

public class
    SearchManager<TAnalyzer, TDirectory, TTaxoDirectory> : ISearchManager<TAnalyzer, TDirectory, TTaxoDirectory>
    where TAnalyzer : Analyzer
    where TDirectory : Directory
    where TTaxoDirectory : FSDirectory
{
    /// <summary>
    /// 分析器
    /// </summary>
    public virtual TAnalyzer Analyzer { get; }

    /// <summary>
    /// 普通索引存储路径
    /// </summary>
    public virtual TDirectory Directory { get; }

    /// <summary>
    /// 维度索引存储路径
    /// </summary>
    public virtual TTaxoDirectory TaxoDirectory { get; }

    private Spatial4n.Context.SpatialContext _spatialContext;
    private readonly SpatialStrategy _spatialStrategy;

    public SearchManager(TDirectory directory, TAnalyzer analyzer, TTaxoDirectory taxoDirectory)
    {
        Directory = directory;
        Analyzer = analyzer;
        TaxoDirectory = taxoDirectory;

        //一个IndexWriterConfig实例只能供一个IndexWriter实例使用，因此不能将IndexWriterConfig作为局部公共变量使用，所以下方代码错误。
        //IndexConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);

        _spatialContext = Spatial4n.Context.SpatialContext.Geo;
        SpatialPrefixTree grid = new GeohashPrefixTree(_spatialContext, CoreConstant.DefaultGeoMaxLevels);
        _spatialStrategy = new RecursivePrefixTreeStrategy(grid, CoreConstant.DefaultGeoField);
    }

    #region 创建索引

    /// <summary>
    /// 创建索引
    /// </summary>
    /// <param name="fields">Field域集合，也可以是其他结构</param>
    public virtual void CreateIndex(Dictionary<string, object> fields)
    {
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, Analyzer);
        using var writer = new IndexWriter(Directory, config);
        //创建文档
        var doc = new Document();
        foreach (var field in fields)
        {
            var f = GetField(field);
            doc.Add(f);
        }

        //索引操作器增加文档
        writer.AddDocument(doc);
        //刷新索引
        writer.Flush(true, true);
        writer.Commit();
        // writer.Dispose();
    }

    #endregion

    #region 创建索引

    private static Field GetField(KeyValuePair<string, object> field)
    {
        var values = field.Value;
        Field f1 = new StringField(field.Key, field.Value.ToString(), Field.Store.YES);
        switch (values.GetType())
        {
            case { } t when t == typeof(int):
                f1 = new Int32Field(field.Key, (int)values, Field.Store.YES);

                break;
            case { } t when t == typeof(long):
                f1 = new Int64Field(field.Key, (long)values, Field.Store.YES);

                break;
            case { } t when t == typeof(double):
                f1 = new DoubleField(field.Key, (double)values, Field.Store.YES);

                break;

            case { } t when t == typeof(DateTime):
                var timestamp = ((DateTime)field.Value).ToUniversalTime().Ticks;
                f1 = new Int64Field(field.Key, timestamp, Field.Store.YES);

                break;
            case { } t when t == typeof(string):
                f1 = new StringField(field.Key, field.Value.ToString(), Field.Store.YES);

                break;
            default:
                f1 = new StringField(field.Key, field.Value.ToString(), Field.Store.YES);

                break;
        }

        return f1;
    }

    /// <summary>
    /// 批量创建索引
    /// </summary>
    /// <param name="fields">Field域集合，也可以是其他结构</param>
    /// <param name="uniKey">主键</param>
    public virtual void CreateIndex(List<Dictionary<string, object>> fields, string uniKey = "id")
    {
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, Analyzer);
        using var writer = new IndexWriter(Directory, config);
        var docList = new List<Document>();
        var terms = new List<Term>();
        foreach (var fieldDic in fields)
        {
            if (fieldDic.TryGetValue(uniKey, out var value))
            {
                // 删除旧文档
                var term = new Term("id", value.ToString());
                terms.Add(term);
            }
            else
            {
                throw new Exception("请放入uniKey的值");
            }

            //创建文档
            var doc = new Document();

            foreach (var field in fieldDic)
            {
                var f = GetField(field);
                doc.Add(f);
            }

            docList.Add(doc);
        }

        // 先移除
        writer.DeleteDocuments(terms.ToArray());
        //索引操作器增加文档
        writer.AddDocuments(docList);
        //刷新索引
        writer.Flush(true, true);
        writer.Commit();
    }

    /// <summary>
    /// 批量创建索引
    /// </summary>
    /// <param name="fields">Field域集合，也可以是其他结构</param>
    /// <param name="version"></param>
    /// <param name="uniKey">主键</param>
    public virtual void CreateIndex(List<Dictionary<string, object>> fields, string version, string uniKey = "id")
    {
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, Analyzer);
        using var writer = new IndexWriter(Directory, config);
        var docList = new List<Document>();
        var terms = new List<Term>();
        foreach (var fieldDic in fields)
        {
            // 删除旧文档
            var term = new Term("id", fieldDic.FirstOrDefault(c => c.Key == uniKey).Value.ToString());
            terms.Add(term);
            //writer.DeleteDocuments(term);


            //创建文档
            var doc = new Document();

            foreach (var field in fieldDic)
            {
                var f = GetField(field);
                doc.Add(f);
            }

            docList.Add(doc);
        }

        // 先移除
        writer.DeleteDocuments(terms.ToArray());
        //索引操作器增加文档
        writer.AddDocuments(docList);


        // 创建一个匹配所有文档的查询
        var matchAllDocsQuery = new MatchAllDocsQuery();

        // 创建一个排除特定条件的查询
        var vTerm = new Term("version", version);
        var notQuery = new TermQuery(vTerm);

        // 使用BooleanQuery组合这两个查询
        var booleanQuery = new BooleanQuery
        {
            { matchAllDocsQuery, Occur.MUST },
            { notQuery, Occur.MUST_NOT }
        };
        // 移除历史版本的数据
        writer.DeleteDocuments(booleanQuery);
        //刷新索引
        writer.Flush(true, true);
        writer.Commit();
    }

    #endregion

    #region 删除所有索引

    /// <summary>
    /// 删除所有索引
    /// </summary>
    public virtual void DeleteAllIndex()
    {
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, Analyzer);
        using var writer = new IndexWriter(Directory, config);
        DirectoryReader.Open(Directory);
        writer.DeleteAll();
        writer.Commit();
    }

    #endregion

    #region 删除索引

    /// <summary>
    /// 删除索引
    /// </summary>
    /// <param name="query"></param>
    public virtual void DeleteIndex(Query query)
    {
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, Analyzer);
        using var writer = new IndexWriter(Directory, config);
        writer.DeleteDocuments(query);
        //刷新索引
        writer.Flush(true, true);
        writer.Commit();
    }

    /// <summary>
    /// 删除索引
    /// </summary>
    /// <param name="docId">文档Id</param>
    public virtual void DeleteIndex(int docId)
    {
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, Analyzer);
        using var writer = new IndexWriter(Directory, config);
        IndexReader reader = DirectoryReader.Open(Directory);
        writer.TryDeleteDocument(reader, docId);
        //刷新索引
        writer.Flush(true, true);
        writer.Commit();
    }

    #endregion

    #region 关键词查询

    /// <summary>
    /// 关键词查询
    /// </summary>
    /// <param name="keyword">关键词</param>
    /// <param name="field">域</param>
    /// <returns>文档,得分</returns>
    public virtual Dictionary<Document, float> SearchIndex(string keyword, string field)
    {
        //定义返回数据结构
        var dic = new Dictionary<Document, float>();
        //实例化索引读取器
        using var reader = DirectoryReader.Open(Directory);
        //实例化索引检索器
        var searcher = new IndexSearcher(reader);
        //创建查询生成器。此处使用了基础的查询生成器QueryParser，构造函数参数为版本、查询的域、分析器。
        //也可以使用MultiFieldQueryParser多域查询生成器。
        var parser = new QueryParser(LuceneVersion.LUCENE_48, field, new StandardAnalyzer(LuceneVersion.LUCENE_48));
        //生成查询对象。
        var query = parser.Parse(keyword);
        //检索并返回结果。检索条件为返回符合程度最高的前10条。
        var matches = searcher.Search(query, 10);
        //遍历检索结构，构造需要返回的数据结构
        foreach (var match in matches.ScoreDocs)
        {
            //获取匹配结果中的文档
            var doc = searcher.Doc(match.Doc);
            //获取匹配文档的分数
            dic.Add(doc, match.Score);
        }

        return dic;
    }

    public virtual Query BuildQuery(List<QuerySearchDto> queryList)
    {
        var booleanQuery = new BooleanQuery();
        foreach (var item in queryList)
        {
            if (item.IsIntRange)
            {
                var query = NumericRangeQuery.NewInt32Range(item.Field, 1, null, true, true);
                booleanQuery.Add(query, item.Occur);
            }
            else
            {
                var query = new WildcardQuery(new Term(item.Field, item.Keyword));
                booleanQuery.Add(query, item.Occur);
            }
        }

        return booleanQuery;
    }

    /// <summary>
    /// 检查击中数量
    /// </summary>
    /// <param name="queryList"></param>
    /// <returns></returns>
    public virtual List<QuerySearchDto> CheckHitCount(List<QuerySearchDto> queryList)
    {
        //定义返回数据结构
        var result = new List<QuerySearchDto>();
        //实例化索引读取器
        using var reader = DirectoryReader.Open(Directory);
        foreach (var item in queryList)
        {
            //实例化索引检索器
            var searcher = new IndexSearcher(reader);
            var query = BuildQuery(new List<QuerySearchDto> { item });
            var matches = searcher.Search(query, 500);
            item.HitCount = matches.TotalHits;
            if (item.HitCount > 0)
            {
                result.Add(item);
            }
        }

        return result;
    }

    /// <summary>
    /// Query对象查询  自定义查询条件
    /// </summary>
    /// <param name="query">关键词</param>
    /// <returns></returns>
    public virtual List<SearchDocDto> SearchIndex(Query query)
    {
        //定义返回数据结构
        var result = new List<SearchDocDto>();
        //实例化索引读取器
        using var reader = DirectoryReader.Open(Directory);
        //实例化索引检索器
        var searcher = new IndexSearcher(reader);

        var matches = searcher.Search(query, 500);
        //遍历检索结构，构造需要返回的数据结构
        foreach (var match in matches.ScoreDocs)
        {
            //获取匹配结果中的文档
            var doc = searcher.Doc(match.Doc);
            //获取匹配文档的分数
            result.Add(new SearchDocDto { DocId = match.Doc, Document = doc, Score = match.Score });
        }

        return result;
    }

    #endregion

    #region 更新索引

    /// <summary>
    /// 更新索引
    /// </summary>
    /// <param name="doc">文档</param>
    /// <param name="dic">要更新的域</param>
    public virtual void UpdateIndex(Document doc, Dictionary<string, string> dic)
    {
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, Analyzer);
        using var writer = new IndexWriter(Directory, config);
        foreach (var term in dic)
        {
            writer.UpdateDocument(new Term(term.Key, term.Value), doc);
        }

        writer.Flush(true, true);
        writer.Commit();
    }

    #endregion

    #region 索引计数

    /// <summary>
    /// 索引计数
    /// </summary>
    /// <returns></returns>
    public virtual int CountIndex()
    {
        //实例化索引读取器
        using var reader = DirectoryReader.Open(Directory);
        return reader.NumDocs;
    }

    #endregion

    #region 为实体创建索引

    /// <summary>
    /// 为实体对象创建索引
    /// </summary>
    /// <param name="entity">实体</param>
    /// <param name="isFiltered">是否启用属性过滤，默认开启</param>
    /// <param name="isCreate">新增或更新，默认新增</param>
    [Obsolete("Obsolete")]
    public virtual void CreateIndexByEntity(IEntity<string> entity, bool isFiltered = true, bool isCreate = true)
    {
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, Analyzer);
        if (isCreate)
        {
            config.OpenMode = OpenMode.CREATE;
        }
        else
        {
            config.OpenMode = OpenMode.CREATE_OR_APPEND;
        }

        using var writer = new IndexWriter(Directory, config);
        var doc = new Document();
        //创建文档
        var type = entity.GetType();
        //为实体所在的类名和Id创建Field，目的是对实体进行标识，便于以后检索
        //文档Document是域Field的集合，本身并无标识。通过添加Id的域对文档进行标识。
        //检索结束后可以从匹配的文档中反向找出对应的数据库实体，与业务建立关联
        doc.Add(new StringField(CoreConstant.EntityType, type.AssemblyQualifiedName, Field.Store.YES)); //添加表名/类名的域
        doc.Add(new StringField(CoreConstant.EntityId, entity.Id, Field.Store.YES)); //添加记录Id/标识的域
        var properties = type.GetProperties();
        //遍历实体的成员集合
        foreach (var propertyInfo in properties)
        {
            var propertyValue = propertyInfo.GetValue(entity);
            if (propertyValue == null)
            {
                continue;
            }

            var fieldName = propertyInfo.Name; //成员字段名称

            if (isFiltered)
            {
                var attributes = propertyInfo.GetCustomAttributes<IndexAttribute>(); //获取自定义属性集合
                foreach (var attribute in attributes)
                {
                    if (propertyValue.IsNull() || string.IsNullOrEmpty(propertyValue.ToString()))
                    {
                        continue;
                    }

                    switch (attribute.FieldType)
                    {
                        case FieldDataType.DateTime:
                            doc.Add(new StringField(fieldName,
                                ((DateTime)propertyValue).ToString("yyyy-MM-dd HH:mm:ss"), attribute.IsStore));
                            break;
                        case FieldDataType.DateYear:
                            doc.Add(new StringField(fieldName, propertyValue.ToString(), attribute.IsStore));
                            break;
                        case FieldDataType.Int32:
                            doc.Add(new Int32Field(fieldName, (int)propertyValue, attribute.IsStore));
                            break;
                        case FieldDataType.Int64:
                            doc.Add(new Int64Field(fieldName, (long)propertyValue, attribute.IsStore));
                            break;
                        case FieldDataType.Double:
                            doc.Add(new DoubleField(fieldName, (double)propertyValue, attribute.IsStore));
                            break;

                        case FieldDataType.Dic:
                            var dic = new Dictionary<string, string>();
                            foreach (var kv in dic)
                            {
                                doc.Add(new TextField(kv.Key, kv.Value, attribute.IsStore));
                            }

                            break;
                        case FieldDataType.Text:
                            doc.Add(new TextField(fieldName, propertyValue.ToString(), attribute.IsStore));
                            break;
                        case FieldDataType.Facet:
                            doc.Add(new FacetField(fieldName, propertyValue.ToString()));
                            break;
                        case FieldDataType.Geo:
                            var point = (IPoint)propertyValue;
                            doc.Add(new StoredField(_spatialStrategy.FieldName,
                                point.X.ToString(CultureInfo.InvariantCulture) + " " +
                                point.Y.ToString(CultureInfo.InvariantCulture)));
                            break;
                        default:
                            doc.Add(new StringField(fieldName, propertyValue.ToString(), attribute.IsStore));
                            break;
                    }
                }
            }
            else
            {
                switch (propertyValue)
                {
                    case DateTime time:
                        doc.Add(new StringField(fieldName, time.ToString("yyyy-MM-dd HH:mm:ss"), Field.Store.YES));
                        break;
                    case int num:
                        doc.Add(new Int32Field(fieldName, num, Field.Store.YES));
                        break;
                    case long num:
                        doc.Add(new Int64Field(fieldName, num, Field.Store.YES));
                        break;
                    case double num:
                        doc.Add(new DoubleField(fieldName, num, Field.Store.YES));
                        break;
                    default:
                        doc.Add(new TextField(fieldName, propertyValue.ToString(), Field.Store.YES));
                        break;
                }
            }
        }

        using (var taxonomyWriter = new DirectoryTaxonomyWriter(TaxoDirectory))
        {
            var facetsConfig = new FacetsConfig();
            doc = facetsConfig.Build(taxonomyWriter, doc);
        }

        if (writer.Config.OpenMode == OpenMode.CREATE)
        {
            writer.AddDocument(doc);
        }
        else
        {
            writer.UpdateDocument(new Term(CoreConstant.EntityId, entity.Id), doc);
        }

        //刷新索引
        writer.Flush(true, true);
        writer.Commit();
    }

    #endregion

    #region 简单查询

    /// <summary>
    /// 简单查询
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    public SingleSearchResult SingleSearch(SingleSearchOption option)
    {
        var result = new SingleSearchResult();
        var watch = Stopwatch.StartNew();
        using (var reader = DirectoryReader.Open(Directory))
        {
            //实例化索引检索器
            var searcher = new IndexSearcher(reader);
            var queryParser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48, option.Fields.ToArray(), Analyzer);
            var query = queryParser.Parse(option.Keyword);
            var matches = searcher.Search(query, option.MaxHits).ScoreDocs;

            #region 高亮

            var scorer = new QueryScorer(query);
            var highlighter = new Highlighter(scorer);

            #endregion

            result.TotalHits = matches.Count();
            foreach (var match in matches)
            {
                var doc = searcher.Doc(match.Doc);
                var item = new SearchResultItem();
                item.Score = match.Score;
                item.EntityId = doc.GetField(CoreConstant.EntityId).GetStringValue();
                item.EntityName = doc.GetField(CoreConstant.EntityType).GetStringValue();
                var storedField = doc.Get(option.Fields[0]);
                if (option.IsHightLight) //高亮
                {
                    var stream = TokenSources.GetAnyTokenStream(reader, match.Doc, option.Fields[0], doc, Analyzer);
                    IFragmenter fragmenter = new SimpleSpanFragmenter(scorer);
                    highlighter.TextFragmenter = fragmenter;
                    var fragment = highlighter.GetBestFragment(stream, storedField);
                    item.FieldValue = fragment;
                }
                else
                {
                    item.FieldValue = storedField;
                }

                result.Items.Add(item);
            }
        }

        watch.Stop();
        result.Elapsed = watch.ElapsedMilliseconds;
        return result;
    }

    #endregion

    #region 关键词分割

    /// <summary>
    /// 关键词分割
    /// </summary>
    /// <param name="keyword"></param>
    /// <returns></returns>
    private List<string> Cut(string keyword)
    {
        var result = new List<string> { keyword }; //先将关键词放入分割结果中
        if (keyword.Length <= 2) //如果关键词过短则不分割，直接返回结果
        {
            return result;
        }

        //常用关键词查询规则替换，‘+’替换并，‘-’替换否，空格替换或
        keyword = keyword.Replace("AND ", "+").Replace("NOT ", "-").Replace("OR ", " ");

        result.AddRange(Regex.Matches(keyword, @""".+""").Select(m =>
        {
            keyword = keyword.Replace(m.Value, "");
            return m.Value;
        })); //必须包含的
        result.AddRange(Regex.Matches(keyword, @"\s-.+\s?").Select(m =>
        {
            keyword = keyword.Replace(m.Value, "");
            return m.Value.Trim();
        })); //必须不包含的

        result.AddRange(Regex.Matches(keyword, @"[\u4e00-\u9fa5]+").Select(m => m.Value)); //中文
        result.AddRange(Regex.Matches(keyword, @"\p{P}?[A-Z]*[a-z]*[\p{P}|\p{S}]*")
            .Select(m => m.Value)); //英文单词
        result.AddRange(Regex.Matches(keyword, "([A-z]+)([0-9.]+)")
            .SelectMany(m => m.Groups.Cast<Group>().Select(g => g.Value))); //英文+数字
        //result.AddRange(new JiebaSegmenter().Cut(keyword, true));//结巴分词
        result.RemoveAll(s => s.Length < 2);
        result = result.Distinct().OrderByDescending(s => s.Length).Take(10).ToList();

        return result;
    }

    /// <summary>
    /// 关键词分割
    /// </summary>
    /// <param name="keyword"></param>
    /// <param name="isDefault"></param>
    /// <returns></returns>
    private List<string> Cut(string keyword, bool isDefault)
    {
        var result = new List<string>();
        using var tokenStream = Analyzer.GetTokenStream(null, keyword);
        tokenStream.Reset();
        var attributes = tokenStream.GetAttribute<global::Lucene.Net.Analysis.TokenAttributes.ICharTermAttribute>();
        while (tokenStream.IncrementToken())
        {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < attributes.Length; i++)
            {
                stringBuilder.Append(attributes.Buffer[i]);
            }

            var item = stringBuilder.ToString();
            if (!result.Contains(item))
            {
                result.Add(item);
            }
        }

        return result;
    }

    #endregion

    #region 包含分页的查询

    /// <summary>
    /// 包含分页的查询
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    public PagedSearchResult PagedSearch(PagedSearchOption option)
    {
        var result = new PagedSearchResult();
        var watch = Stopwatch.StartNew();

        using (var reader = DirectoryReader.Open(Directory))
        {
            var searcher = new IndexSearcher(reader);

            var query = BuildQuery(option.QueryList);
            var sortFields = new List<SortField>
            {
                SortField.FIELD_SCORE
            };
            foreach (var sort in option.Sorts)
            {
                if (sort.Field == "updatetime" || sort.Field.IndexOf("time") > -1)
                {
                    sortFields.Add(new SortField(sort.Field, SortFieldType.INT64, sort.Desc));
                }
                else
                {
                    sortFields.Add(new SortField(sort.Field, SortFieldType.STRING, sort.Desc));
                }
            }

            var sorts = new Sort(sortFields.ToArray());
            Expression<Func<ScoreDoc, bool>> whereExpression = m => m.Score >= option.Score;
            var matches = searcher
                .Search(query, null, option.MaxHits, sorts, true, true).ScoreDocs
                .Where(whereExpression.Compile());
            result.TotalHits = matches.Count();
            matches = matches.Skip((option.PageIndex - 1) * option.PageSize);
            matches = matches.Take(option.PageSize);

            foreach (var match in matches)
            {
                var doc = searcher.Doc(match.Doc);
                var item = new PagedSearchResultItem();
                item.Score = match.Score;
                item.Doc = doc;
                result.Items.Add(item);
            }
        }

        watch.Stop();
        result.Elapsed = watch.ElapsedMilliseconds;
        return result;
    }

    /// <summary>
    /// 多分组查询
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public List<PagedSearchResult> PageSearchList(List<PagedSearchOption> options)
    {
        var result = new List<PagedSearchResult>();

        using var reader = DirectoryReader.Open(Directory);
        var searcher = new IndexSearcher(reader);
        foreach (var option in options)
        {
            var watch = Stopwatch.StartNew();
            //// 获取索引中的总文档数量
            //int totalDocuments = reader.MaxDoc;

            //// 获取包含删除文档的总文档数量
            //int totalDocumentsIncludingDeleted = reader.NumDocs;
            var r1 = new PagedSearchResult();
            var query = BuildQuery(option.QueryList);
            var sortFields = new List<SortField>();
            foreach (var sort in option.Sorts)
            {
                if (sort.Field == "updatetime" || sort.Field.IndexOf("time") > -1)
                {
                    sortFields.Add(new SortField(sort.Field, SortFieldType.INT64, sort.Desc));
                }
                else
                {
                    sortFields.Add(new SortField(sort.Field, SortFieldType.STRING, sort.Desc));
                }
            }

            //sortFields.Add(SortField.FIELD_SCORE);
            var sorts = new Sort(sortFields.ToArray());
            Expression<Func<ScoreDoc, bool>> whereExpression = m => m.Score >= -1;
            var matches = searcher.Search(query, null, option.MaxHits, sorts, true, true).ScoreDocs
                .Where(whereExpression.Compile());
            r1.TotalHits = matches.Count();
            matches = matches.Skip((option.PageIndex - 1) * option.PageSize);
            matches = matches.Take(option.PageSize);
            r1.SearchOption = option;
            foreach (var match in matches)
            {
                var doc = searcher.Doc(match.Doc);
                var item = new PagedSearchResultItem();
                item.Score = match.Score;
                item.Doc = doc;
                r1.Items.Add(item);
            }

            watch.Stop();
            r1.Elapsed = watch.ElapsedMilliseconds;
            result.Add(r1);
        }


        return result;
    }

    #endregion

    #region 包含权重的查询

    /// <summary>
    /// 包含权重的查询
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    public ScoredSearchResult ScoredSearch(ScoredSearchOption option)
    {
        var result = new ScoredSearchResult();
        var watch = Stopwatch.StartNew(); //启动计时器

        using (var reader = DirectoryReader.Open(Directory))
        {
            var searcher = new IndexSearcher(reader);
            var queryParser = new MultiFieldQueryParser(LuceneVersion.LUCENE_48, option.Fields.ToArray(), Analyzer,
                option.Boosts);
            var terms = Cut(option.Keyword); //关键词分割
            Query query = QueryExpression(queryParser, terms); //查询语句拼接扩展
            var sort = new Sort(SortField.FIELD_SCORE); //默认按照评分排序
            Expression<Func<ScoreDoc, bool>> whereExpression = m => m.Score >= option.Score;
            var matches = searcher.Search(query, option.Filter, option.MaxHits, sort, true, true).ScoreDocs
                .Where(whereExpression.Compile());

            foreach (var match in matches)
            {
                var doc = searcher.Doc(match.Doc);
                var item = new SearchResultItem
                {
                    Score = match.Score,
                    EntityId = doc.Get(CoreConstant.EntityId),
                    EntityName = doc.Get(CoreConstant.EntityType)
                };
                result.Items.Add(item);
            }

            result.TotalHits = matches.Count();
        }

        watch.Stop(); //停止计时器
        result.Elapsed = watch.ElapsedMilliseconds;
        return result;
    }

    #endregion

    #region 查询语句扩展

    /// <summary>
    /// 查询语句扩展
    /// </summary>
    /// <param name="queryParser"></param>
    /// <param name="terms"></param>
    /// <returns></returns>
    private BooleanQuery QueryExpression(MultiFieldQueryParser queryParser, List<string> terms)
    {
        var query = new BooleanQuery();
        foreach (var term in terms)
        {
            if (term.StartsWith("\""))
            {
                query.Add(queryParser.Parse(term.Trim('"')), Occur.MUST); //必须匹配
            }
            else if (term.StartsWith("-"))
            {
                query.Add(queryParser.Parse(term), Occur.MUST_NOT); //必须不匹配
            }
            else
            {
                query.Add(queryParser.Parse(term.Replace("~", "") + "~"), Occur.SHOULD); //可以匹配
            }
        }

        return query;
    }

    #endregion

    #region 计数查询

    public virtual List<CountSearchResultItem> CountSearch(CountSearchOption option)
    {
        var items = new List<CountSearchResultItem>();
        using var reader = DirectoryReader.Open(Directory);
        //实例化索引检索器
        var searcher = new IndexSearcher(reader);
        var fields = MultiFields.GetFields(reader);

        fields.GetTerms(option.FieldName);
        var collection = searcher.CollectionStatistics(option.FieldName);
        reader.GetSumTotalTermFreq(option.FieldName);
        return items;
    }

    #endregion

    #region 维度查询

    /// <summary>
    /// 维度查询
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    public FacetSearchResult FacetSearch(FacetSearchOption option)
    {
        var result = new FacetSearchResult();
        using var reader = DirectoryReader.Open(Directory);
        var taxonomyReader = new DirectoryTaxonomyReader(TaxoDirectory);
        var searcher = new IndexSearcher(reader);
        var facetsCollector = new FacetsCollector();

        FacetsCollector.Search(searcher, new MatchAllDocsQuery(), 10, facetsCollector);
        Facets facets = new FastTaxonomyFacetCounts(taxonomyReader, new FacetsConfig(), facetsCollector);

        foreach (var field in option.Fields)
        {
            var facetResult = facets.GetTopChildren(option.MaxHits, field);
            result.Items.Add(facetResult);
        }

        return result;
    }

    #endregion

    #region 分组查询

    /// <summary>
    /// 分组查询
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    public GroupSearchResult GroupSearch(GroupSearchOption option)
    {
        var result = new GroupSearchResult();
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        using var reader = DirectoryReader.Open(Directory);
        var searcher = new IndexSearcher(reader);
        var groupingSearch = new GroupingSearch(option.Fields.FirstOrDefault()); //指定要进行分组的索引
        //groupingSearch.SetGroupSort(new Sort(SortField.FIELD_SCORE));//设置分组排序规则
        groupingSearch.SetCachingInMB(4.0, true); //设置缓存空间大小
        groupingSearch.SetAllGroups(true); //设置是否为所有分组
        groupingSearch.SetFillSortFields(true); //设置是否填充排序值
        //groupingSearch.SetGroupDocsLimit(option.MaxHits);//设置分组文档个数限制

        Query query = new MatchAllDocsQuery();

        var groups = groupingSearch.Search(searcher, query, 0, option.MaxHits);

        stopwatch.Stop();
        result.Elapsed = stopwatch.ElapsedMilliseconds;

        return result;
    }

    #endregion

    #region 地理信息查询

    /// <summary>
    /// 地理信息查询
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    [Obsolete("Obsolete")]
    public GeoSearchResult GeoSearch(GeoSearchOption option)
    {
        var result = new GeoSearchResult();
        var watch = Stopwatch.StartNew();
        using (var reader = DirectoryReader.Open(Directory))
        {
            //实例化索引检索器
            var searcher = new IndexSearcher(reader);
            var args = new SpatialArgs(SpatialOperation.Intersects,
                _spatialContext.MakeCircle(option.Origin,
                    DistanceUtils.Dist2Degrees(option.Raidus, DistanceUtils.EARTH_EQUATORIAL_RADIUS_KM)));
            var filter = _spatialStrategy.MakeFilter(args);
            Query query = new MatchAllDocsQuery();
            var topDocs = searcher.Search(query, filter, option.MaxHits);
            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                var doc = searcher.Doc(scoreDoc.Doc);
                result.Items.Add(doc, scoreDoc.Score);
            }
        }

        watch.Stop();
        result.Elapsed = watch.ElapsedMilliseconds;
        return result;
    }

    #endregion

    #region 获取索引基本信息

    /// <summary>
    /// 获取索引基本信息
    /// </summary>
    /// <returns></returns>
    public IndexInfo Info()
    {
        var info = new IndexInfo();
        using var reader = DirectoryReader.Open(Directory);
        var fields = MultiFields.GetIndexedFields(reader);
        var fsdDirectory = (FSDirectory)reader.Directory;
        var directoryInfo = fsdDirectory.Directory;
        var files = directoryInfo.GetFiles();
        info.IndexPath = directoryInfo.FullName;
        info.FileNum = files.Length;
        info.DocumentNum = reader.NumDocs;
        info.FieldNum = fields.Count;
        foreach (var field in fields)
        {
            info.TermNum += reader.GetSumTotalTermFreq(field);
        }

        info.UpdateTime = directoryInfo.LastWriteTime;
        info.Version = reader.Version;
        return info;
    }

    #endregion
}

public class SearchManager : SearchManager<Analyzer, Directory, FSDirectory>, ISearchManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="analyzer"></param>
    /// <param name="taxoDirectory"></param>
    public SearchManager(Directory directory, Analyzer analyzer, FSDirectory taxoDirectory) : base(directory, analyzer,
        taxoDirectory)
    {
    }
}