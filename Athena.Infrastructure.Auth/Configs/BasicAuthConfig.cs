namespace Athena.Infrastructure.Auth.Configs;

/// <summary>
/// 基础认证配置
/// </summary>
public class BasicAuthConfig
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = null!;
}