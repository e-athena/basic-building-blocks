namespace Athena.Infrastructure.FreeRedis.Test.Caching;

public class CacheManagerTest : TestBase
{
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
}