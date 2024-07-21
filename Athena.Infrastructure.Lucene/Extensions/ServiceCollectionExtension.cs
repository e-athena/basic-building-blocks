using Athena.Infrastructure.Lucene.Analyzers;
using Athena.Infrastructure.Lucene.Interfaces;
using JiebaNet.Segmenter;
using Microsoft.Extensions.DependencyInjection;

namespace Athena.Infrastructure.Lucene.Extensions;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// 添加搜索管理器
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddLuceneSearchManager(
        this IServiceCollection services, SearchManagerConfig config)
    {
        services.AddSingleton(config);
        services.AddSingleton(global::Lucene.Net.Store.FSDirectory.Open(config.FacetPath));
        services.AddSingleton<global::Lucene.Net.Store.Directory>(global::Lucene.Net.Store.FSDirectory.Open(config.DefaultPath));
        services.AddSingleton<global::Lucene.Net.Analysis.Analyzer>(new JieBaAnalyzer(TokenizerMode.Search));
        services.AddTransient<ISearchManager, SearchManager>();
        return services;
    }
}