namespace Athena.Infrastructure.Jwt;

/// <summary>
/// 
/// </summary>
public interface ISecurityContextAccessor
{
    /// <summary>
    /// APPID
    /// </summary>
    string? AppId { get; }

    /// <summary>
    /// MemberId
    /// </summary>
    string? MemberId { get; }

    /// <summary>
    /// 用户Id
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    string? RealName { get; }

    /// <summary>
    /// 用户帐号
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// 是否为开发者
    /// </summary>
    bool IsRoot { get; }

    /// <summary>
    /// 是否为租户管理员
    /// </summary>
    bool IsTenantAdmin { get; }

    /// <summary>
    /// 租户Id
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// 角色
    /// </summary>
    string? Role { get; }

    /// <summary>
    /// 角色列表
    /// </summary>
    IList<string>? Roles { get; }

    /// <summary>
    /// 角色名称
    /// </summary>
    string? RoleName { get; }

    /// <summary>
    /// 角色名称列表
    /// </summary>
    IList<string>? RoleNames { get; }

    /// <summary>
    /// 是否刷新缓存
    /// </summary>
    bool IsRefreshCache { get; }

    /// <summary>
    /// JwtToken
    /// </summary>
    string JwtToken { get; }

    /// <summary>
    /// JwtTokenNotBearer
    /// </summary>
    string JwtTokenNotBearer { get; }

    /// <summary>
    /// 是否已登录
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// 浏览器信息
    /// </summary>
    string UserAgent { get; }

    /// <summary>
    /// Ip地址
    /// </summary>
    string IpAddress { get; }

    /// <summary>
    /// 创建Token
    /// </summary>
    /// <param name="claims"></param>
    /// <returns>Token</returns>
    string CreateToken(List<Claim> claims);

    /// <summary>
    /// 创建Token
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="scheme"></param>
    /// <returns>Token</returns>
    string CreateToken(List<Claim> claims, string scheme);

    /// <summary>
    /// 验证Token
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="token">要验证的Token，为null时读取请求头上的Authorization</param>
    /// <param name="principal">token中存储的数据</param>
    /// <returns></returns>
    bool ValidateToken(JwtConfig config, string? token, out ClaimsPrincipal? principal);

    /// <summary>
    /// 创建Token
    /// </summary>
    /// <param name="config"></param>
    /// <param name="claims"></param>
    /// <param name="hasScheme"></param>
    /// <param name="scheme"></param>
    /// <returns>Token</returns>
    string CreateToken(JwtConfig config, List<Claim> claims, bool hasScheme = true,
        string scheme = JwtBearerDefaults.AuthenticationScheme);

    /// <summary>
    /// 创建Token
    /// </summary>
    /// <param name="config"></param>
    /// <param name="claims"></param>
    /// <returns></returns>
    string CreateTokenNotScheme(JwtConfig config, List<Claim> claims);

    /// <summary>
    /// 创建Token
    /// </summary>
    /// <param name="claims"></param>
    /// <returns>Token</returns>
    string CreateTokenNotScheme(List<Claim> claims);
}