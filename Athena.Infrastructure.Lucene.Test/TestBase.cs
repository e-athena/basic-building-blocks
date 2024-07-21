using System.Security.Claims;
using Athena.Infrastructure.Lucene.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnassignedGetOnlyAutoProperty
namespace Athena.Infrastructure.Lucene.Test;

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
    protected TService GetService<TService>()
    {
        return Provider.GetService<TService>() ?? throw new ArgumentNullException(nameof(TService));
    }

    /// <summary>
    /// 
    /// </summary>
    protected TestBase()
    {
        var services = new ServiceCollection();
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            // .AddJsonFile("appsettings.json", false)
            .Build();
        services.AddLuceneSearchManager(new SearchManagerConfig
        {
            DefaultPath = "defaultPath",
            FacetPath = "facetPath",
            DictPath = "dictPath"
        });
        RegistryServices(services);
        Provider = services.BuildServiceProvider();
    }

    protected virtual void RegistryServices(IServiceCollection services)
    {
    }
}