using System.Globalization;
using Athena.Infrastructure.Lucene.Interfaces;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Xunit;
using Xunit.Abstractions;

namespace Athena.Infrastructure.Lucene.Test;

public class Tests : TestBase
{
    private readonly ITestOutputHelper _testOutputHelper;

    /// <summary>
    ///
    /// </summary>
    /// <param name="testOutputHelper"></param>
    public Tests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    // 编写ISearchManager的单元测试
    [Fact]
    public void TestISearchManager()
    {
        // Arrange
        var searchManager = GetService<ISearchManager>();

        // 移除索引
        searchManager.DeleteAllIndex();

        var fieldsList = new List<Dictionary<string, object>>
        {
            new()
            {
                { "id", 123 },
                { "name", "test11" }
            }
        };

        // Act
        searchManager.CreateIndex(fieldsList);

        var result = searchManager.SearchIndex("t*", "name");

        // Assert
        Assert.Single(result);

        // 检查Document是否正确
        Assert.Equal("test11", result.First().Key.Get("name"));
        Assert.Equal("123", result.First().Key.Get("id"));

        // 打印Document
        foreach (var item in result)
        {
            _testOutputHelper.WriteLine(item.Key.ToString());
            _testOutputHelper.WriteLine(item.Value.ToString(CultureInfo.InvariantCulture));
        }

        //生成查询对象。
        var query = new QueryParser(LuceneVersion.LUCENE_48, "name", new StandardAnalyzer(LuceneVersion.LUCENE_48))
            .Parse("t*");
        var result1 = searchManager.SearchIndex(query);

        // Assert
        Assert.Single(result1);
        Assert.Equal("test11", result1.First().Document.Get("name"));
        Assert.Equal("123", result1.First().Document.Get("id"));
    }
}