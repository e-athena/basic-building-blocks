namespace Athena.Infrastructure.ViewModels;

/// <summary>
/// CascaderViewModel
/// </summary>
public class CascaderViewModel
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Label { get; set; } = null!;

    /// <summary>
    /// 值
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// 是否禁用
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// 子项
    /// </summary>
    public List<CascaderViewModel>? Children { get; set; }
}