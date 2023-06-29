namespace Athena.Infrastructure.ApiPermission.Models;

/// <summary>
/// 应用资源信息
/// </summary>
public class ApplicationResourceInfo
{
    /// <summary>
    /// 应用名称
    /// </summary>
    public string ApplicationName { get; set; } = null!;

    /// <summary>
    /// 应用ID
    /// </summary>
    public string ApplicationId { get; set; } = null!;

    /// <summary>
    /// 资源列表
    /// </summary>
    public IList<MenuTreeInfo> Resources { get; set; } = null!;
}