// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 扩展
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 添加微信API客户端工厂
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomWeChatApiClientFactory(
        this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<IWechatApiClientFactory, WechatApiClientFactory>();
        return services;
    }

    /// <summary>
    /// 添加微信API
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomWeChatApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions();
        var configs = GetWeChatMpConfigs(configuration);
        services.Configure<List<WeChatMpConfig>>(opts => { opts.AddRange(configs); });
        var config = configuration.GetWeChatMpConfig();
        services.Configure<WeChatMpConfig>(opts =>
        {
            opts.AppId = config.AppId;
            opts.AppSecret = config.AppSecret;
        });
        services.Configure<WechatApiClientOptions>(opts =>
        {
            opts.AppId = config.AppId;
            opts.AppSecret = config.AppSecret;
        });
        var officialAccountConfig = configuration.GetWeChatOfficialAccountConfig();
        services.Configure<WeChatOfficialAccountConfig>(opts =>
        {
            opts.AppId = officialAccountConfig.AppId;
            opts.AppSecret = officialAccountConfig.AppSecret;
            opts.Token = officialAccountConfig.Token;
            opts.EncodingAesKey = officialAccountConfig.EncodingAesKey;
        });
        services.AddCustomWeChatApiClientFactory();
        return services;
    }

    /// <summary>
    /// 读取微信小程序配置
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <returns></returns>
    private static WeChatMpConfig GetWeChatMpConfig(
        this IConfiguration configuration,
        string configVariable = "WeChatMpConfig",
        string envVariable = "WE_CHAT_MP_CONFIG")
    {
        return configuration.GetConfig<WeChatMpConfig>(configVariable, envVariable);
    }

    /// <summary>
    /// 读取微信小程序配置
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <returns></returns>
    private static List<WeChatMpConfig> GetWeChatMpConfigs(
        this IConfiguration configuration,
        string configVariable = "WeChatMpConfigs",
        string envVariable = "WE_CHAT_MP_CONFIGS")
    {
        return configuration.GetConfig<List<WeChatMpConfig>>(configVariable, envVariable);
    }

    /// <summary>
    /// 读取微信公众号配置
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <returns></returns>
    private static WeChatOfficialAccountConfig GetWeChatOfficialAccountConfig(
        this IConfiguration configuration,
        string configVariable = "WeChatOfficialAccountConfig",
        string envVariable = "WE_CHAT_OFFICIAL_ACCOUNT_CONFIG")
    {
        return configuration.GetConfig<WeChatOfficialAccountConfig>(configVariable, envVariable);
    }
}