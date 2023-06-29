// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class Extensions
{
    private static IServiceCollection AddCustomAuth(this IServiceCollection services,
        IConfiguration config,
        Action<JwtBearerOptions>? configureOptions = null,
        Action<IServiceCollection>? configureMoreActions = null)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                config.Bind("JwtBearer", options);
                configureOptions?.Invoke(options);
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                policy.RequireClaim(ClaimTypes.Name);
            });
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
    public static IServiceCollection AddCustomJwtAuth(this IServiceCollection services,
        IConfiguration configuration,
        Action<JwtBearerOptions>? configureOptions = null,
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
        return services.AddCustomAuth(configuration, options =>
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

            configureOptions?.Invoke(options);
        }, configureMoreActions);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="configureOptions"></param>
    /// <param name="configureMoreActions"></param>
    /// <returns></returns>
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
        return configuration.GetConfig<JwtConfig>(configVariable, envVariable);
    }
}