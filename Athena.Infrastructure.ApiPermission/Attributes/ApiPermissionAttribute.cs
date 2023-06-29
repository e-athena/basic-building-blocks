namespace Athena.Infrastructure.ApiPermission.Attributes;

/// <summary>
/// Api接口权限属性
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ApiPermissionAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public ApiPermissionAttribute()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="isVisible">是否可见</param>
    public ApiPermissionAttribute(bool isVisible)
    {
        IsVisible = isVisible;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="alias">别名</param>
    public ApiPermissionAttribute(string alias)
    {
        Alias = alias;
    }

    /// <summary>
    /// 别名
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否可见
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// 附加规则
    /// </summary>
    public string[]? AdditionalRules { get; set; }
}