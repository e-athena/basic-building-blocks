namespace Athena.Infrastructure.Mvc.Messaging.Requests;

/// <summary>
/// 登录请求类
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    public string Password { get; set; } = null!;

    /// <summary>
    /// 客户端ID
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// 记住我
    /// </summary>
    public bool RememberMe { get; set; }
}