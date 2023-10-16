using System.Reflection;
using System.Security.Claims;
using Athena.Infrastructure.Attributes;
using Athena.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Athena.Infrastructure.SqlSugar.AspNetCore.Test;

public class TestBase
{
    /// <summary>
    /// 
    /// </summary>
    protected IConfigurationRoot Configuration { get; private set; }

    /// <summary>
    /// 服务提供程序，用于获取服务实例
    /// </summary>
    protected IServiceProvider Provider { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    protected TService? GetService<TService>()
    {
        return Provider.GetService<TService>();
    }

    /// <summary>
    /// 
    /// </summary>
    protected TestBase()
    {
        var services = new ServiceCollection();
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .Build();
        services.AddCustomServiceComponent();
        services.AddCustomMediatR("Athena");
        services.AddHttpContextAccessor();
        services.AddLogging();
        services.AddCustomSqlSugar(Configuration);
        RegistryServices(services);
        Provider = services.BuildServiceProvider();
    }

    protected virtual void RegistryServices(IServiceCollection services)
    {
    }
}

[Component]
public class DefaultSecurityContextAccessor : ISecurityContextAccessor
{
    public DefaultSecurityContextAccessor()
    {
    }

    public DefaultSecurityContextAccessor(string? tenantId)
    {
        TenantId = tenantId;
    }

    public string? AppId { get; }
    public string? MemberId { get; }
    public string? UserId { get; } = "asdasdqwewqeqw";
    public string? RealName { get; }
    public string? UserName { get; }
    public bool IsRoot { get; }
    public bool IsTenantAdmin { get; }
    public string? TenantId { get; }
    public string? Role { get; }
    public IList<string>? Roles { get; }
    public string? RoleName { get; }
    public IList<string>? RoleNames { get; }
    public bool IsRefreshCache { get; }
    public string JwtToken { get; }
    public string JwtTokenNotBearer { get; }
    public bool IsAuthenticated { get; }
    public string UserAgent { get; }
    public string IpAddress { get; }

    public string CreateToken(List<Claim> claims)
    {
        throw new NotImplementedException();
    }

    public string CreateToken(List<Claim> claims, string scheme)
    {
        throw new NotImplementedException();
    }

    public bool ValidateToken(JwtConfig config, string? token, out ClaimsPrincipal? principal)
    {
        throw new NotImplementedException();
    }

    public string CreateToken(JwtConfig config, List<Claim> claims, bool hasScheme = true,
        string scheme = JwtBearerDefaults.AuthenticationScheme)
    {
        throw new NotImplementedException();
    }

    public string CreateTokenNotScheme(JwtConfig config, List<Claim> claims)
    {
        throw new NotImplementedException();
    }

    public string CreateTokenNotScheme(List<Claim> claims)
    {
        throw new NotImplementedException();
    }
}