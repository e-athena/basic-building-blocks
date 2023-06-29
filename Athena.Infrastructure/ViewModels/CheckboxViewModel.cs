namespace Athena.Infrastructure.ViewModels;

/// <summary>
/// 
/// </summary>
public class CheckboxViewModel
{
    /// <summary>
    /// Label
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
    /// 是否选中
    /// </summary>
    public bool Checked { get; set; }

    /// <summary>
    /// 扩展
    /// </summary>
    public string? Extend { get; set; }
}