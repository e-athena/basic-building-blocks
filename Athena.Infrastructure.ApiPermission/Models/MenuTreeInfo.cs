namespace Athena.Infrastructure.ApiPermission.Models;

/// <summary>
/// 菜单树信息
/// </summary>
public class MenuTreeInfo
{
    /// <summary>
    /// 所属应用
    /// </summary>
    public string AppId { get; set; } = null!;

    /// <summary>
    /// 上级Code
    /// </summary>
    public string? ParentCode { get; set; }

    /// <summary>
    /// 路由路径
    /// </summary>
    public string Path { get; set; } = "/";

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 是否可见
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// 是否需要授权访问
    /// </summary>
    public bool IsAuth { get; set; } = true;

    /// <summary>
    /// 排序值
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 子菜单
    /// </summary>
    public List<MenuTreeInfo>? Children { get; set; }

    /// <summary>
    /// 功能
    /// </summary>
    public List<FunctionInfo>? Functions { get; set; }
}