namespace Athena.Infrastructure.FreeRedis.Test;

public class TestBase
{
    /// <summary>
    /// 
    /// </summary>
    protected IConfigurationRoot Configuration { get; private set; }

    /// <summary>
    /// 服务提供程序，用于获取服务实例
    /// </summary>
    protected IServiceProvider Provider { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    protected TService? GetService<TService>()
    {
        return Provider.GetService<TService>();
    }

    /// <summary>
    /// 
    /// </summary>
    protected TestBase()
    {
        var service = new ServiceCollection();
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .Build();
        service.AddCustomFreeRedisCache(Configuration);
        Provider = service.BuildServiceProvider();
    }
}