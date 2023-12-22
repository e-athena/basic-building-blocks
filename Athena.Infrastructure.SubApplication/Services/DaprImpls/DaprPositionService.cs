namespace Athena.Infrastructure.SubApplication.Services.DaprImpls;

/// <summary>
/// 角色服务接口Dapr实现
/// </summary>
public class DaprPositionService : DaprServiceBase, IPositionService
{
    private readonly DaprClient _daprClient;
    private readonly ServiceCallConfig _config;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="daprClient"></param>
    /// <param name="accessor"></param>
    public DaprPositionService(IOptions<ServiceCallConfig> options, DaprClient daprClient,
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
    [ServiceInvokeExceptionLogging]
    public async Task<List<SelectViewModel>> GetSelectListAsync(string? organizationId = null)
    {
        var methodName = "/api/SubApplication/GetPositionSelectList";
        if (!string.IsNullOrEmpty(organizationId))
        {
            methodName += $"?organizationId={organizationId}";
        }

        var result = await _daprClient
            .InvokeMethodAsync<ApiResult<List<SelectViewModel>>>(HttpMethod.Get, _config.AppId, GetMethodName(methodName));

        return result.Data ?? new List<SelectViewModel>();
    }
}