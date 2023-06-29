namespace Athena.Infrastructure.ViewModels;

/// <summary>
/// 树形下拉选择框
/// </summary>
public class TreeSelectViewModel
{
    /// <summary>
    /// ID
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// 父级Id
    /// </summary>
    public string? ParentId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 值
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// 是否禁用
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否是叶子节点
    /// </summary>
    public bool IsLeaf { get; set; }

    /// <summary>
    /// 是否拥有权限
    /// </summary>
    public bool Checked { get; set; }

    /// <summary>
    /// 子项
    /// </summary>
    public List<TreeSelectViewModel>? Children { get; set; }

    /// <summary>
    /// 扩展
    /// </summary>
    public string? Extend { get; set; }
}