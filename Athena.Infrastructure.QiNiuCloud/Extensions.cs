using System.Diagnostics;
using Athena.Infrastructure.QiNiuCloud;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 扩展
/// </summary>
public static class Extensions
{
    /// <summary>
    /// AddCustomQiNiuKoDoService
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomQiNiuKoDoService(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddOptions();
        var config = configuration.GetQiNiuConfig();
        // 配置
        services.Configure<QiNiuConfig>(opts =>
        {
            opts.Bucket = config.Bucket;
            opts.AccessKey = config.AccessKey;
            opts.SecretKey = config.SecretKey;
            opts.PolicyExpires = config.PolicyExpires;
            opts.CdnAddress = config.CdnAddress;
        });
        services.AddSingleton<IQiNiuKoDoService, QiNiuKoDoService>();
        return services;
    }

    /// <summary>
    /// 读取七牛配置
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <returns></returns>
    private static QiNiuConfig GetQiNiuConfig(
        this IConfiguration configuration,
        string configVariable = "QiNiuConfig",
        string envVariable = "QI_NIU_CONFIG")
    {
        var config = configuration.GetOptions<QiNiuConfig>(configVariable);
        var env = Environment.GetEnvironmentVariable(envVariable);
        if (!string.IsNullOrEmpty(env))
        {
            config = JsonConvert.DeserializeObject<QiNiuConfig>(env);
        }

        if (config == null)
        {
            throw new Exception("七牛配置为空");
        }

        return config;
    }

    [DebuggerStepThrough]
    private static TModel GetOptions<TModel>(this IConfiguration configuration, string section) where TModel : new()
    {
        var model = new TModel();
        configuration.GetSection(section).Bind(model);
        return model;
    }
}