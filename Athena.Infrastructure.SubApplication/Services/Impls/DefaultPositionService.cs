namespace Athena.Infrastructure.SubApplication.Services.Impls;

/// <summary>
/// 角色服务接口默认实现
/// </summary>
public class DefaultPositionService : DefaultServiceBase, IPositionService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="accessor"></param>
    public DefaultPositionService(ISecurityContextAccessor accessor) : base(accessor)
    {
    }

    /// <summary>
    /// 读取下拉列表
    /// </summary>
    /// <param name="organizationId"></param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<List<SelectViewModel>> GetSelectListAsync(string? organizationId = null)
    {
        var url = "/api/SubApplication/GetPositionSelectList";
        if (!string.IsNullOrEmpty(organizationId))
        {
            url += $"?organizationId={organizationId}";
        }

        var result = await GetRequest(url)
            .GetJsonAsync<ApiResult<List<SelectViewModel>>>();
        return result.Data ?? new List<SelectViewModel>();
    }
}