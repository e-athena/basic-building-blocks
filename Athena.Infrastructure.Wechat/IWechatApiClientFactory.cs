namespace Athena.Infrastructure.Wechat;

/// <summary>
/// 微信API客户端工厂接口
/// </summary>
public interface IWechatApiClientFactory
{
    /// <summary>
    /// 创建客户端
    /// </summary>
    /// <returns></returns>
    WechatApiClient CreateClient();

    /// <summary>
    /// 创建客户端
    /// </summary>
    /// <param name="appId">AppId</param>
    /// <param name="appSecret">AppSecret</param>
    /// <returns></returns>
    WechatApiClient CreateClient(string appId, string appSecret);

    /// <summary>
    /// 获取AccessToken
    /// </summary>
    /// <returns></returns>
    Task<string> GetAccessTokenAsync();

    /// <summary>
    /// 获取AccessToken
    /// </summary>
    /// <param name="appId">AppId</param>
    /// <param name="appSecret">AppSecret</param>
    /// <returns></returns>
    Task<string> GetAccessTokenAsync(string appId, string appSecret);

    /// <summary>
    /// 获取AccessToken
    /// </summary>
    /// <returns></returns>
    Task<string> GetAccessTokenAsync(WechatApiClient client);
}