namespace Athena.Infrastructure.Wechat;

/// <summary>
/// 微信公众号配置
/// </summary>
public class WeChatOfficialAccountConfig
{
    /// <summary>
    /// Token
    /// </summary>
    public string? Token { set; get; }

    /// <summary>
    /// 加密AESKey
    /// </summary>
    public string? EncodingAesKey { set; get; }

    /// <summary>
    /// AppID
    /// </summary>
    public string AppId { set; get; } = null!;

    /// <summary>
    /// AppSecret
    /// </summary>
    public string AppSecret { set; get; } = null!;
}