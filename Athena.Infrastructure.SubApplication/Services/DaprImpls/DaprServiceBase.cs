namespace Athena.Infrastructure.SubApplication.Services.DaprImpls;

/// <summary>
/// 
/// </summary>
public class DaprServiceBase
{
    private readonly ISecurityContextAccessor _accessor;

    public DaprServiceBase(ISecurityContextAccessor accessor)
    {
        _accessor = accessor;
    }

    /// <summary>
    /// 读取方法名称
    /// </summary>
    /// <param name="methodName"></param>
    /// <returns></returns>
    protected string GetMethodName(string methodName)
    {
        // 生成AppId参数和TenantId参数，如果methodName带有?，则添加&，否则添加?
        var appId = _accessor.AppId;
        var tenantId = _accessor.TenantId;

        var param = $"appId={appId}&tenant_id={tenantId}";

        return methodName.Contains('?') ? $"{methodName}&{param}" : $"{methodName}?{param}";
    }
}