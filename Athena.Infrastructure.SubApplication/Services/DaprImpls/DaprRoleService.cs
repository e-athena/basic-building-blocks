namespace Athena.Infrastructure.SubApplication.Services.DaprImpls;

/// <summary>
/// 角色服务接口Dapr实现
/// </summary>
public class DaprRoleService : DaprServiceBase, IRoleService
{
    private readonly DaprClient _daprClient;
    private readonly ServiceCallConfig _config;

    public DaprRoleService(IOptions<ServiceCallConfig> options, DaprClient daprClient,
        ISecurityContextAccessor accessor) : base(accessor)
    {
        _daprClient = daprClient;
        _config = options.Value;
    }

    /// <summary>
    /// 读取下拉列表
    /// </summary>
    /// <param name="organizationId"></param>
    /// <returns></returns>
    public async Task<List<SelectViewModel>> GetSelectListAsync(string? organizationId = null)
    {
        var methodName = "/api/SubApplication/GetRoleSelectList";
        if (!string.IsNullOrEmpty(organizationId))
        {
            methodName += $"?organizationId={organizationId}";
        }

        var result = await _daprClient
            .InvokeMethodAsync<ApiResult<List<SelectViewModel>>>(HttpMethod.Get, _config.AppId, GetMethodName(methodName));

        return result.Data ?? new List<SelectViewModel>();
    }
}