// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 添加基础认证
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomBasicAuth(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddOptions();
        var config = configuration.GetConfig<BasicAuthConfig>("BasicAuthConfig", "BASIC_AUTH_CONFIG");
        services.AddCustomBasicAuth(config.UserName, config.Password);
        return services;
    }

    /// <summary>
    /// 添加基础认证
    /// </summary>
    /// <param name="services"></param>
    /// <param name="userName">用户名</param>
    /// <param name="password">密码</param>
    /// <returns></returns>
    public static IServiceCollection AddCustomBasicAuth(
        this IServiceCollection services,
        string userName,
        string password
    )
    {
        services.Configure<BasicAuthConfig>(cfg =>
        {
            cfg.UserName = userName;
            cfg.Password = password;
        });
        return services;
    }

    /// <summary>
    /// 添加自定义认证
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="configureOptions"></param>
    /// <param name="authorizationOptions"></param>
    /// <param name="authenticationBuilderActions"></param>
    /// <param name="configureMoreActions"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomAuth(this IServiceCollection services,
        IConfiguration configuration,
        Action<JwtBearerOptions>? configureOptions = null,
        Action<AuthorizationOptions>? authorizationOptions = null,
        Action<AuthenticationBuilder>? authenticationBuilderActions = null,
        Action<IServiceCollection>? configureMoreActions = null)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ISecurityContextAccessor, SecurityContextAccessor>();
        var config = configuration.GetJwtConfig();
        services.Configure<JwtConfig>(cfg =>
        {
            cfg.Audience = config.Audience;
            cfg.ValidateAudience = config.ValidateAudience;
            cfg.Issuer = config.Issuer;
            cfg.ValidateIssuer = config.ValidateIssuer;
            cfg.SecurityKey = config.SecurityKey;
            cfg.ValidateIssuerSigningKey = config.ValidateIssuerSigningKey;
            cfg.Expires = config.Expires;
            cfg.ValidateLifetime = config.ValidateLifetime;
        });
        var authentication = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // SecurityKey
                var issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SecurityKey));
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = config.ValidateIssuer, // 是否验证Issuer
                    ValidateAudience = config.ValidateAudience, // 是否验证Audience
                    ValidateLifetime = config.ValidateLifetime, // 是否验证失效时间
                    ValidateIssuerSigningKey = config.ValidateIssuerSigningKey, // 是否验证SecurityKey
                    ValidAudience = config.Audience, // Audience
                    ValidIssuer = config.Issuer, // Issuer，这两项和前面签发jwt的设置一致
                    IssuerSigningKey = issuerSigningKey
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // 从headers中读取Authorization，如果包含Bearer，则取出token
                        var accessToken = context
                            .Request
                            .Headers["Authorization"]
                            .ToString()
                            .Replace("Bearer ", "");

                        // 如果headers中没有Authorization，则从query中读取access_token
                        if (string.IsNullOrEmpty(accessToken))
                        {
                            accessToken = context.Request.Query["access_token"];
                        }

                        if (!string.IsNullOrEmpty(accessToken) && string.IsNullOrEmpty(context.Token))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
                options.ForwardDefaultSelector = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/cap"))
                    {
                        return CapCookieAuthenticationDefaults.AuthenticationScheme;
                    }

                    return JwtBearerDefaults.AuthenticationScheme;
                };
                configureOptions?.Invoke(options);
            })
            .AddCookie(CapCookieAuthenticationDefaults.AuthenticationScheme,
                options =>
                {
                    // 
                    options.LoginPath = "/cap/login";
                }
            );
        // 额外的配置
        authenticationBuilderActions?.Invoke(authentication);

        services.AddAuthorization(options =>
        {
            options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireClaim(ClaimTypes.NameIdentifier);
            });
            options.AddPolicy(CapCookieAuthenticationDefaults.AuthenticationScheme, policy =>
            {
                policy.AddAuthenticationSchemes(CapCookieAuthenticationDefaults.AuthenticationScheme);
                policy.RequireClaim(ClaimTypes.NameIdentifier);
            });
            // 额外的配置
            authorizationOptions?.Invoke(options);
        });
        configureMoreActions?.Invoke(services);

        return services;
    }

    /// <summary>
    /// 添加Jwt认证
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="configureOptions"></param>
    /// <param name="configureMoreActions"></param>
    /// <returns></returns>
    [Obsolete("请使用AddCustomAuth")]
    public static IServiceCollection AddCustomJwtAuth(this IServiceCollection services,
        IConfiguration configuration,
        Action<JwtBearerOptions>? configureOptions = null,
        Action<IServiceCollection>? configureMoreActions = null)
    {
        return services.AddCustomAuth(
            configuration,
            configureOptions: configureOptions,
            configureMoreActions: configureMoreActions
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="configureOptions"></param>
    /// <param name="configureMoreActions"></param>
    /// <returns></returns>
    [Obsolete("请使用AddCustomAuth")]
    public static IServiceCollection AddCustomJwtAuthWithSignalR(this IServiceCollection services,
        IConfiguration configuration,
        Action<JwtBearerOptions>? configureOptions = null,
        Action<IServiceCollection>? configureMoreActions = null)
    {
        return services.AddCustomJwtAuth(configuration, options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    // If the request is for our hub...
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/hubs"))
                    {
                        // Read the token out of the query string
                        context.Token = accessToken.ToString();
                    }

                    return Task.CompletedTask;
                }
            };
            configureOptions?.Invoke(options);
        }, configureMoreActions);
    }


    /// <summary>
    /// 读取JWT配置
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <returns></returns>
    private static JwtConfig GetJwtConfig(
        this IConfiguration configuration,
        string configVariable = "JwtBearer",
        string envVariable = "JWT_CONFIG")
    {
        var config = configuration.GetConfig<JwtConfig>(configVariable, envVariable);
        config.Check();
        return config;
    }
}