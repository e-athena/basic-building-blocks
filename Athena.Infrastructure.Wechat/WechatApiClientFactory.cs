using Athena.Infrastructure.Caching;
using SKIT.FlurlHttpClient.Wechat.Api.Models;

namespace Athena.Infrastructure.Wechat;

/// <summary>
/// 微信API客户端工厂接口实现类
/// </summary>
public class WechatApiClientFactory : IWechatApiClientFactory
{
    private readonly IOptions<WechatApiClientOptions> _wechatApiClientOptions;
    private readonly ICacheManager _cacheManager;

    public WechatApiClientFactory(
        IHttpClientFactory httpClientFactory,
        IOptions<WechatApiClientOptions> wechatApiClientOptions, ICacheManager cacheManager)
    {
        httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _wechatApiClientOptions =
            wechatApiClientOptions ?? throw new ArgumentNullException(nameof(wechatApiClientOptions));
        _cacheManager = cacheManager;

        FlurlHttp.GlobalSettings.FlurlClientFactory = new DelegatingFlurlClientFactory(httpClientFactory);
    }

    /// <summary>
    /// 创建客户端
    /// </summary>
    /// <returns></returns>
    public WechatApiClient CreateClient()
    {
        return new WechatApiClient(_wechatApiClientOptions.Value);
    }

    public WechatApiClient CreateClient(string appId, string appSecret)
    {
        return new WechatApiClient(new WechatApiClientOptions
        {
            AppId = appId,
            AppSecret = appSecret
        });
    }

    public Task<string> GetAccessTokenAsync()
    {
        return GetAccessTokenAsync(_wechatApiClientOptions.Value.AppId, _wechatApiClientOptions.Value.AppSecret);
    }

    public Task<string> GetAccessTokenAsync(string appId, string appSecret)
    {
        var key = $"{appId}:access_token";
        return GetAccessTokenAsync(CreateClient(appId, appSecret), key);
    }

    public Task<string> GetAccessTokenAsync(WechatApiClient client)
    {
        var key = $"{client.Credentials.AppId}:access_token";
        return GetAccessTokenAsync(client, key);
    }

    private async Task<string> GetAccessTokenAsync(WechatApiClient client, string key)
    {
        var accessToken = await _cacheManager.GetOrCreateAsync(key, async () =>
        {
            var res = await client.ExecuteCgibinTokenAsync(new CgibinTokenRequest());
            if (res.IsSuccessful())
            {
                return res.AccessToken;
            }

            throw new Exception(res.ErrorMessage);
        }, TimeSpan.FromMinutes(119));

        return accessToken!;
    }

    public Task<bool> RefreshAccessTokenAsync(string appId, string appSecret)
    {
        var key = $"{appId}:access_token";
        return RefreshAccessTokenAsync(CreateClient(appId, appSecret), key);
    }

    public Task<bool> RefreshAccessTokenAsync(WechatApiClient client)
    {
        var key = $"{client.Credentials.AppId}:access_token";
        return RefreshAccessTokenAsync(client, key);
    }

    private async Task<bool> RefreshAccessTokenAsync(WechatApiClient client, string key)
    {
        var res = await client.ExecuteCgibinTokenAsync(new CgibinTokenRequest());
        if (!res.IsSuccessful())
        {
            throw new Exception(res.ErrorMessage);
        }

        await _cacheManager.SetStringAsync(key, res.AccessToken, TimeSpan.FromMinutes(119));
        return true;
    }
}

/// <summary>
/// DelegatingFlurlClientFactory
/// </summary>
internal class DelegatingFlurlClientFactory : IFlurlClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DelegatingFlurlClientFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public IFlurlClient Get(Flurl.Url url)
    {
        return new FlurlClient(_httpClientFactory.CreateClient(url.ToUri().Host));
    }

    public void Dispose()
    {
        // Do Nothing
    }
}