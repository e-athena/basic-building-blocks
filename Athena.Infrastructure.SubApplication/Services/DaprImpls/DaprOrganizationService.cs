namespace Athena.Infrastructure.SubApplication.Services.DaprImpls;

/// <summary>
/// 组织架构服务接口Dapr实现
/// </summary>
public class DaprOrganizationService : DaprServiceBase, IOrganizationService
{
    private readonly DaprClient _daprClient;
    private readonly ServiceCallConfig _config;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="daprClient"></param>
    /// <param name="accessor"></param>
    public DaprOrganizationService(IOptions<ServiceCallConfig> options, DaprClient daprClient,
        ISecurityContextAccessor accessor) : base(accessor)
    {
        _daprClient = daprClient;
        _config = options.Value;
    }

    /// <summary>
    /// 读取级联列表
    /// </summary>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<List<CascaderViewModel>> GetCascaderListAsync(string? parentId = null)
    {
        var methodName = "/api/SubApplication/GetOrganizationCascaderList";
        if (!string.IsNullOrEmpty(parentId))
        {
            methodName += $"?organizationId={parentId}";
        }

        var result = await _daprClient
            .InvokeMethodAsync<ApiResult<List<CascaderViewModel>>>(HttpMethod.Get, _config.AppId, GetMethodName(methodName));

        return result.Data ?? new List<CascaderViewModel>();
    }

    /// <summary>
    /// 读取树形列表
    /// </summary>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<List<TreeViewModel>> GetTreeListAsync(string? parentId = null)
    {
        var methodName = "/api/SubApplication/GetOrganizationTreeList";
        if (!string.IsNullOrEmpty(parentId))
        {
            methodName += $"?organizationId={parentId}";
        }

        var result = await _daprClient
            .InvokeMethodAsync<ApiResult<List<TreeViewModel>>>(HttpMethod.Get, _config.AppId, GetMethodName(methodName));

        return result.Data ?? new List<TreeViewModel>();
    }

    /// <summary>
    /// 读取下拉列表
    /// </summary>
    /// <param name="parentId"></param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<List<SelectViewModel>> GetSelectListAsync(string? parentId = null)
    {
        var methodName = "/api/SubApplication/GetOrganizationSelectList";
        if (!string.IsNullOrEmpty(parentId))
        {
            methodName += $"?organizationId={parentId}";
        }

        var result = await _daprClient
            .InvokeMethodAsync<ApiResult<List<SelectViewModel>>>(HttpMethod.Get, _config.AppId, GetMethodName(methodName));

        return result.Data ?? new List<SelectViewModel>();
    }

    /// <summary>
    /// 读取Id
    /// </summary>
    /// <param name="name">名称</param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<string?> GetIdByNameAsync(string name)
    {
        var methodName = $"/api/SubApplication/GetOrganizationIdByName?organizationName={name}";
        var result = await _daprClient
            .InvokeMethodAsync<ApiResult<string>>(HttpMethod.Get, _config.AppId, GetMethodName(methodName));

        return result.Data;
    }
}