using Athena.Infrastructure.ColumnPermissions;
using Athena.Infrastructure.DataPermission;
using Athena.Infrastructure.SubApplication.Services;
using Athena.Infrastructure.SubApplication.Services.CommonImpls;
using Athena.Infrastructure.SubApplication.Services.DaprImpls;
using Athena.Infrastructure.SubApplication.Services.Impls;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加子应用服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="daprConfigure"></param>
    /// <returns></returns>
    public static IServiceCollection AddSubApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DaprClientBuilder>? daprConfigure = null
    )
    {
        // 读取配置
        var config = configuration.GetConfig<ServiceCallConfig>(
            nameof(ServiceCallConfig),
            callback: opt =>
            {
                // 检查配置是否正确
                opt.Check();
            }
        );
        services.Configure<ServiceCallConfig>(options =>
        {
            options.AppId = config.AppId;
            options.HttpApiUrl = config.HttpApiUrl;
            options.CallType = config.CallType;
        });
        // 通用服务
        services.AddScoped<ICommonService, DefaultCommonService>();
        services.AddScoped<IColumnPermissionService, DefaultColumnPermissionService>();

        if (config.CallType == ServiceCallType.Http)
        {
            // 添加服务
            services.AddScoped<IOrganizationService, DefaultOrganizationService>();
            services.AddScoped<IPositionService, DefaultPositionService>();
            services.AddScoped<IRoleService, DefaultRoleService>();
            services.AddScoped<IUserService, DefaultUserService>();
            services.AddScoped<ITenantService, HttpTenantService>();
            services.AddScoped<IDataPermissionService, DefaultDataPermissionService>();
            return services;
        }

        if (config.CallType != ServiceCallType.Dapr)
        {
            return services;
        }

        // 添加Dapr客户端
        services.AddDaprClient(daprConfigure);
        // 添加服务
        services.AddScoped<IOrganizationService, DaprOrganizationService>();
        services.AddScoped<IPositionService, DaprPositionService>();
        services.AddScoped<IRoleService, DaprRoleService>();
        services.AddScoped<IUserService, DaprUserService>();
        services.AddScoped<ITenantService, DaprTenantService>();
        services.AddScoped<IDataPermissionService, DaprDataPermissionService>();

        return services;
    }
}

/// <summary>
/// 服务调用配置
/// </summary>
public class ServiceCallConfig
{
    /// <summary>
    /// APPID
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// http API地址
    /// </summary>
    public string? HttpApiUrl { get; set; }

    /// <summary>
    /// 调用方式
    /// </summary>
    public ServiceCallType CallType { get; set; }

    /// <summary>
    /// 超时时间/秒
    /// </summary>
    public int Timeout { get; set; } = 30;

    /// <summary>
    /// 检查配置是否正确，1、CallType为Dapr时，AppId不能为空，2、CallType为Http时，HttpApiUrl不能为空
    /// </summary>
    public void Check()
    {
        if (CallType == ServiceCallType.Dapr)
        {
            if (string.IsNullOrEmpty(AppId))
            {
                throw new ArgumentNullException(nameof(AppId), "AppId不能为空");
            }
        }

        if (CallType == ServiceCallType.Http)
        {
            if (string.IsNullOrEmpty(HttpApiUrl))
            {
                throw new ArgumentNullException(nameof(HttpApiUrl), "HttpApiUrl不能为空");
            }
        }
    }
}

/// <summary>
/// 调用方式：0、Http，1、Dapr
/// </summary>
public enum ServiceCallType
{
    /// <summary>
    /// HTTP
    /// </summary>
    [Description("HTTP")] Http = 0,

    /// <summary>
    /// DAPR
    /// </summary>
    [Description("DAPR")] Dapr = 1
}