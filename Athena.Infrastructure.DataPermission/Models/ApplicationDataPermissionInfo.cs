using Athena.Infrastructure.ViewModels;

namespace Athena.Infrastructure.DataPermission.Models;

/// <summary>
/// 应用数据权限
/// </summary>
public class ApplicationDataPermissionInfo
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
    /// 额外的下拉列表
    /// </summary>
    public IList<SelectViewModel> ExtraSelectList { get; set; } = new List<SelectViewModel>();

    /// <summary>
    /// 数据权限组列表
    /// </summary>
    public IList<DataPermissionGroup> DataPermissionGroups { get; set; } = new List<DataPermissionGroup>();
}