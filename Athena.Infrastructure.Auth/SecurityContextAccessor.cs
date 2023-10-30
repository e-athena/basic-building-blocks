namespace Athena.Infrastructure.Auth;

/// <summary>
/// 
/// </summary>
public class SecurityContextAccessor : ISecurityContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SecurityContextAccessor> _logger;
    private readonly JwtConfig _jwtConfig;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="loggerFactory"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public SecurityContextAccessor(
        IOptions<JwtConfig> options,
        IHttpContextAccessor httpContextAccessor,
        ILoggerFactory loggerFactory
    )
    {
        _jwtConfig = options.Value;
        _logger = loggerFactory.CreateLogger<SecurityContextAccessor>();
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    /// APPID
    /// </summary>
    public string? AppId
    {
        get
        {
            var appId = _httpContextAccessor.HttpContext?.Request.Headers["AppId"];
            if (!string.IsNullOrEmpty(appId))
            {
                return appId;
            }

            appId = _httpContextAccessor.HttpContext?.User.FindFirst("AppId")?.Value;
            return (!string.IsNullOrEmpty(appId)
                ? appId.ToString()
                : _httpContextAccessor.HttpContext?.Request.Query["app_id"].ToString()) ?? null;
        }
    }

    /// <summary>
    /// 会员ID
    /// </summary>
    public string? MemberId => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// 用户ID
    /// </summary>
    public string? UserId => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

    /// <summary>
    /// 真实姓名
    /// </summary>
    public string? RealName => _httpContextAccessor.HttpContext?.User.FindFirst("RealName")?.Value;

    /// <summary>
    /// 是否为开发者
    /// </summary>
    public bool IsRoot => UserName == "root";

    /// <summary>
    /// 是否为租户管理员
    /// </summary>
    public bool IsTenantAdmin => _httpContextAccessor.HttpContext?.User.FindFirst("IsTenantAdmin")?.Value == "true";

    /// <summary>
    ///  角色
    /// </summary>
    public string? Role => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

    /// <summary>
    /// 角色列表
    /// </summary>
    public IList<string>? Roles => string.IsNullOrEmpty(Role) ? null : Role.Split(',').ToList();

    /// <summary>
    /// 角色名称
    /// </summary>
    public string? RoleName => _httpContextAccessor.HttpContext?.User.FindFirst("RoleName")?.Value;

    /// <summary>
    /// 角色名称列表
    /// </summary>
    public IList<string>? RoleNames => string.IsNullOrEmpty(RoleName) ? null : RoleName.Split(',').ToList();

    /// <summary>
    /// 是否刷新缓存
    /// </summary>
    public bool IsRefreshCache
    {
        get
        {
            var queryValue = _httpContextAccessor.HttpContext?.Request.Query["refresh_cache"];
            if (bool.TryParse(queryValue, out var flag1) && flag1)
            {
                return true;
            }

            var headerValue1 = _httpContextAccessor.HttpContext?.Request.Query["refresh_cache"];
            if (bool.TryParse(headerValue1, out var flag2) && flag2)
            {
                return true;
            }

            var headerValue2 = _httpContextAccessor.HttpContext?.Request.Headers["RefreshCache"];
            return bool.TryParse(headerValue2, out var flag3) && flag3;
        }
    }


    /// <summary>
    /// 租户ID
    /// </summary>
    public string? TenantId
    {
        get
        {
            var tenantId = _httpContextAccessor.HttpContext?.Request.Headers["TenantId"];
            if (!string.IsNullOrEmpty(tenantId))
            {
                return tenantId;
            }

            tenantId = _httpContextAccessor.HttpContext?.User.FindFirst("TenantId")?.Value;
            return (!string.IsNullOrEmpty(tenantId)
                ? tenantId.ToString()
                : _httpContextAccessor.HttpContext?.Request.Query["tenant_id"].ToString()) ?? null;
        }
    }

    /// <summary>
    /// JwtToken
    /// </summary>
    public string JwtToken
    {
        get
        {
            var jwtToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"];
            if (jwtToken?.Count > 0)
            {
                return jwtToken.Value.ToString();
            }

            jwtToken = _httpContextAccessor.HttpContext?.Request.Query["access_token"].ToString();

            if (jwtToken?.Count > 0)
            {
                return jwtToken.Value.ToString();
            }

            jwtToken = _httpContextAccessor.HttpContext?.Request.Query["AccessToken"];

            return jwtToken?.Count > 0 ? jwtToken.Value.ToString() : string.Empty;
        }

        // get
        // {
        //     var jwtToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"];
        //     if (!string.IsNullOrEmpty(jwtToken))
        //     {
        //         return jwtToken;
        //     }
        //
        //     jwtToken = _httpContextAccessor.HttpContext?.Request.Query["access_token"].ToString();
        //     return (string.IsNullOrEmpty(jwtToken)
        //         ? _httpContextAccessor.HttpContext?.Request.Query["AccessToken"]
        //         : jwtToken) ?? string.Empty;
        // }
    }

    /// <summary>
    /// JwtTokenNotBearer
    /// </summary>
    public string JwtTokenNotBearer
    {
        get
        {
            if (string.IsNullOrEmpty(JwtToken))
            {
                return string.Empty;
            }

            return !JwtToken.Contains("Bearer") ? JwtToken : JwtToken.Split(" ")[1];
        }
    }

    // /// <summary>
    // /// 浏览器信息
    // /// </summary>
    // public string UserAgent => _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"] ?? string.Empty;

    /// <summary>
    /// 浏览器信息
    /// </summary>
    public string UserAgent
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"];
            return value?.Count > 0 ? value.Value.ToString() : string.Empty;
        }
    }

    // /// <summary>
    // /// Ip地址
    // /// </summary>
    // public string IpAddress => _httpContextAccessor.HttpContext?.Request.Headers["X-Real-IP"] ?? string.Empty;

    /// <summary>
    /// Ip地址
    /// </summary>
    public string IpAddress
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.Request.Headers["X-Real-IP"];
            return value?.Count > 0 ? value.Value.ToString() : string.Empty;
        }
    }

    /// <summary>
    /// 是否已授权
    /// </summary>
    public bool IsAuthenticated
    {
        get
        {
            var isAuthenticated =
                _httpContextAccessor.HttpContext?.User.Identities.FirstOrDefault()?.IsAuthenticated;
            return isAuthenticated.HasValue && isAuthenticated.Value;
        }
    }

    /// <summary>
    /// 创建Token
    /// </summary>
    /// <param name="claims"></param>
    /// <returns></returns>
    public string CreateToken(List<Claim> claims)
    {
        return CreateToken(_jwtConfig, claims);
    }

    /// <summary>
    /// 创建Token
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="scheme"></param>
    /// <returns></returns>
    public string CreateToken(List<Claim> claims, string scheme)
    {
        return CreateToken(_jwtConfig, claims, true, scheme);
    }

    /// <summary>
    /// 验证Token
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="token">要验证的Token，为null时读取请求头上的Authorization</param>
    /// <param name="principal">token中存储的数据</param>
    /// <returns></returns>
    public bool ValidateToken(JwtConfig config, string? token, out ClaimsPrincipal? principal)
    {
        token ??= JwtTokenNotBearer;
        principal = null;
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var now = DateTime.UtcNow;
            if (jwtToken.ValidFrom > now || jwtToken.ValidTo < now)
            {
                return false;
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SecurityKey));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = config.Issuer,
                ValidateAudience = true,
                ValidAudience = config.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true
            };
            principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);

            // 检查AppId是否一致
            var appId = principal.Claims.FirstOrDefault(x => x.Type == "AppId")?.Value;
            if (appId == config.AppId)
            {
                return true;
            }

            _logger.LogWarning("AppId不一致");
            return false;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "验证Token失败");
            return false;
        }
    }

    /// <summary>
    /// 创建Token
    /// </summary>
    /// <param name="config"></param>
    /// <param name="claims"></param>
    /// <param name="hasScheme"></param>
    /// <param name="scheme"></param>
    /// <returns></returns>
    public string CreateToken(JwtConfig config, List<Claim> claims,
        bool hasScheme = true,
        string scheme = JwtBearerDefaults.AuthenticationScheme)
    {
        claims.AddRange(new List<Claim>
        {
            // new(JwtRegisteredClaimNames.Iss, config.Issuer),
            // new(JwtRegisteredClaimNames.Aud, config.Audience),
            // // 这个就是过期时间，目前是过期1000秒，可自定义，注意JWT有自己的缓冲过期时间
            // new(JwtRegisteredClaimNames.Exp, config.Expires.ToString()),
            // new(JwtRegisteredClaimNames.Sub, config.),
            new(JwtRegisteredClaimNames.Nbf, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
            new(JwtRegisteredClaimNames.Iat, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("AppId", config.AppId ?? string.Empty)
        });
        //sign the token using a secret key.This secret will be shared between your API and anything that needs to check that the token is legit.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SecurityKey));
        var creeds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //.NET Core’s JwtSecurityToken class takes on the heavy lifting and actually creates the token.
        var issuer = config.Issuer;
        var audience = config.Audience;

        var tokenObj = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.Now.AddSeconds(config.Expires),
            notBefore: null,
            signingCredentials: creeds);
        var token = new JwtSecurityTokenHandler().WriteToken(tokenObj);
        return hasScheme ? $"{scheme} {token}" : token;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="config"></param>
    /// <param name="claims"></param>
    /// <returns></returns>
    public string CreateTokenNotScheme(JwtConfig config, List<Claim> claims)
    {
        return CreateToken(config, claims, false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="claims"></param>
    /// <returns></returns>
    public string CreateTokenNotScheme(List<Claim> claims)
    {
        return CreateToken(_jwtConfig, claims, false);
    }
}