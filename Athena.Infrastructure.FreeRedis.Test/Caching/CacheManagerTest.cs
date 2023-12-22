using System.Globalization;
using Athena.Infrastructure.Attributes;
using Athena.Infrastructure.Enums;
using Athena.Infrastructure.Utilities;
using Xunit.Abstractions;

namespace Athena.Infrastructure.FreeRedis.Test.Caching;

public class CacheManagerTest : TestBase
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CacheManagerTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test1()
    {
        var cacheManager = GetService<ICacheManager>();
        Xunit.Assert.NotNull(cacheManager);
        const string key = "key1";
        const string value = "hello";
        // set
        cacheManager?.SetString(key, value);
        // get
        var value1 = cacheManager?.GetString(key);
        Xunit.Assert.Equal(value, value1);
        // remove
        cacheManager?.Remove(key);
    }

    [Fact]
    public void Test2()
    {
        var cacheManager = GetService<ICacheManager>();
        Xunit.Assert.NotNull(cacheManager);
        const string key = "key2";
        const string value = "hello";
        // set
        cacheManager?.Set(key, new TestObject {Data = value});
        // get
        var value1 = cacheManager?.Get<TestObject>(key);
        Xunit.Assert.Equal(value, value1!.Data);
        // remove
        cacheManager?.Remove(key);
    }

    private class TestObject
    {
        public string Data { get; set; } = null!;
    }

    [Fact]
    public void Test3()
    {
        var enums = Enum.GetValues(typeof(TestEnum));
        var enumList = enums.OfType<Enum>().ToList();

        for (var i = 0; i < enumList.Count; i++)
        {
            var @enum = enumList[i];
            var status = @enum.ToValueStatus(ValueEnumStatus.Default);
            Xunit.Assert.Equal(status, i == 0 ? ValueEnumStatus.Default : ValueEnumStatus.Error);
            var status1 = @enum.ToValueStatus();
            Xunit.Assert.Equal(status1, i == 0 ? null : ValueEnumStatus.Error);
        }
    }

    [Fact]
    public void Test4()
    {
        var idList = new List<long>();


        // 开10个线程，每个线程生成10个ID，打印线程ID和ID
        Parallel.For(0, 1, i =>
        {
            for (var j = 0; j < 10; j++)
            {
                var instance = Snowflake.Instance;
                var id = instance.NextId();
                idList.Add(id);
                _testOutputHelper.WriteLine($"{i} {id}");
                _testOutputHelper.WriteLine(id.ToString());
                _testOutputHelper.WriteLine(instance.GetTime(id).ToString("yyyy-MM-dd HH:mm:ss fff", CultureInfo.InvariantCulture));
            }
        });

        // for (var i = 0; i < 10; i++)
        // {
        //     var id = Snowflake.Instance.NextId();
        //     idList.Add(id);
        //     _testOutputHelper.WriteLine(id.ToString());
        // }

        var distinctIdList = idList.Distinct().ToList();

        Xunit.Assert.Equal(idList.Count, distinctIdList.Count);
    }
}

public enum TestEnum
{
    Abbb,
    [ValueStatus(ValueEnumStatus.Error)] Bccc
}