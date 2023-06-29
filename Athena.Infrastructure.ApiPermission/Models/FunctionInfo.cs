namespace Athena.Infrastructure.ApiPermission.Models;

/// <summary>
/// 功能信息
/// </summary>
public class FunctionInfo
{
    /// <summary>
    /// 上级Code
    /// </summary>
    public string? ParentCode { get; set; }

    /// <summary>
    /// Key
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 显示名称
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 值列表
    /// </summary>
    public IList<string> Values => string.IsNullOrEmpty(Value) ? new List<string>() : Value.Split(",");

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}