using Athena.Infrastructure.Caching;
using Athena.Infrastructure.Wechat.Exceptions;
using Microsoft.Extensions.Logging;
using SKIT.FlurlHttpClient.Wechat.Api.Models;

namespace Athena.Infrastructure.Wechat;

/// <summary>
/// 微信API客户端工厂接口实现类
/// </summary>
public class WechatApiClientFactory : IWechatApiClientFactory
{
    private readonly IOptions<WechatApiClientOptions> _wechatApiClientOptions;
    private readonly ICacheManager _cacheManager;
    private readonly ILogger<WechatApiClientFactory> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpClientFactory"></param>
    /// <param name="wechatApiClientOptions"></param>
    /// <param name="cacheManager"></param>
    /// <param name="loggerFactory"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public WechatApiClientFactory(
        IHttpClientFactory httpClientFactory,
        IOptions<WechatApiClientOptions> wechatApiClientOptions,
        ICacheManager cacheManager,
        ILoggerFactory loggerFactory)
    {
        httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _wechatApiClientOptions =
            wechatApiClientOptions ?? throw new ArgumentNullException(nameof(wechatApiClientOptions));
        _cacheManager = cacheManager;
        _logger = loggerFactory.CreateLogger<WechatApiClientFactory>();

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

    /// <summary>
    /// 创建客户端
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="appSecret"></param>
    /// <returns></returns>
    public WechatApiClient CreateClient(string appId, string appSecret)
    {
        return new WechatApiClient(new WechatApiClientOptions
        {
            AppId = appId,
            AppSecret = appSecret
        });
    }

    /// <summary>
    /// 获取AccessToken
    /// </summary>
    /// <returns></returns>
    public Task<string> GetAccessTokenAsync()
    {
        return GetAccessTokenAsync(_wechatApiClientOptions.Value.AppId, _wechatApiClientOptions.Value.AppSecret);
    }

    /// <summary>
    /// 获取AccessToken
    /// </summary>
    /// <param name="appId">AppId</param>
    /// <param name="appSecret">AppSecret</param>
    /// <returns></returns>
    public Task<string> GetAccessTokenAsync(string appId, string appSecret)
    {
        var key = $"{appId}:access_token";
        return GetAccessTokenAsync(CreateClient(appId, appSecret), key);
    }

    /// <summary>
    /// 获取AccessToken
    /// </summary>
    /// <param name="client">Api客户端</param>
    /// <returns></returns>
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

            _logger.LogError("获取AccessToken失败，错误码：{ErrorCode}，错误信息：{ErrorMessage}", res.ErrorCode, res.ErrorMessage);
            if (res.ErrorMessage != null && res.ErrorMessage.Contains("not in whitelist"))
            {
                throw new CurrentRequestIpNotInWhitelistException(res.ErrorCode, "当前请求IP不在白名单中");
            }

            // 当前请求IP不在白名单中错误
            throw new ReadWeChatAccountTokenException(res.ErrorCode, res.ErrorMessage ?? "获取AccessToken失败");
        }, TimeSpan.FromMinutes(119));

        return accessToken!;
    }

    /// <summary>
    /// 刷新AccessToken
    /// </summary>
    /// <param name="appId">AppId</param>
    /// <param name="appSecret">AppSecret</param>
    /// <returns></returns>
    public Task<bool> RefreshAccessTokenAsync(string appId, string appSecret)
    {
        var key = $"{appId}:access_token";
        return RefreshAccessTokenAsync(CreateClient(appId, appSecret), key);
    }

    /// <summary>
    /// 刷新AccessToken
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
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
            _logger.LogError("获取AccessToken失败，错误码：{ErrorCode}，错误信息：{ErrorMessage}", res.ErrorCode, res.ErrorMessage);
            if (res.ErrorMessage != null && res.ErrorMessage.Contains("not in whitelist"))
            {
                throw new CurrentRequestIpNotInWhitelistException(res.ErrorCode, "当前请求IP不在白名单中");
            }

            // 当前请求IP不在白名单中错误
            throw new ReadWeChatAccountTokenException(res.ErrorCode, res.ErrorMessage ?? "获取AccessToken失败");
        }

        await _cacheManager.SetStringAsync(key, res.AccessToken, TimeSpan.FromMinutes(119));
        return true;
    }
}