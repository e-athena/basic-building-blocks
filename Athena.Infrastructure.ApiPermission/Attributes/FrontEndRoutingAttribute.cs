namespace Athena.Infrastructure.ApiPermission.Attributes;

/// <summary>
/// 前端路由属性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class FrontEndRoutingAttribute : Attribute
{
    /// <summary>
    /// 前端路由
    /// </summary>
    /// <param name="name"></param>
    public FrontEndRoutingAttribute(string name)
    {
        Name = name;
    }

    #region 模块信息

    /// <summary>
    /// 模块名称
    /// </summary>
    public string ModuleName { get; set; } = "系统模块";

    /// <summary>
    /// 模块Code
    /// </summary>
    public string ModuleCode { get; set; } = "system";

    /// <summary>
    /// 模块路由路径
    /// </summary>
    public string ModuleRoutePath { get; set; } = "/";

    /// <summary>
    /// 模块图标
    /// </summary>
    public string ModuleIcon { get; set; } = "AppstoreOutlined";

    /// <summary>
    /// 模块排序
    /// </summary>
    public int ModuleSort { get; set; }

    #endregion

    /// <summary>
    /// 上级Code
    /// </summary>
    public string? ParentCode { get; set; }

    /// <summary>
    /// Code
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 路由路径
    /// </summary>
    public string RoutePath { get; set; } = "/";

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; } = "FileOutlined";

    /// <summary>
    /// 是否可见
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否验证权限
    /// </summary>
    public bool IsAuth { get; set; } = true;
}