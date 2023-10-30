namespace Athena.Infrastructure.Auths;

/// <summary>
/// JWT配置
/// <para>REF:http://www.ruanyifeng.com/blog/2018/07/json_web_token-tutorial.html</para>
/// </summary>
public class JwtConfig
{
    /// <summary>
    /// APPID
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// 受众
    /// </summary>
    public string Audience { get; set; } = null!;

    /// <summary>
    /// 验证受众
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// 签发人
    /// </summary>
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// 验证签发人
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// 安全钥匙
    /// </summary>
    public string SecurityKey { get; set; } = null!;

    /// <summary>
    /// 验证安全钥匙
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// 过期时间/秒
    /// </summary>
    public int Expires { get; set; }

    /// <summary>
    /// 验证失效时间
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// 检查配置是否完整
    /// </summary>
    /// <exception cref="AggregateException"></exception>
    public void Check()
    {
        Assert.IsNotNullOrEmpty("受众", Audience);
        Assert.IsNotNullOrEmpty("签发人", Audience);
        Assert.IsNotNullOrEmpty("安全钥匙", SecurityKey);

        if (Expires <= 0)
        {
            throw new AggregateException("[过期时间/秒]不能小于1秒");
        }
    }
}