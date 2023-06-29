namespace Athena.Infrastructure.ApiPermission.Models;

/// <summary>
/// 接口权限树形结构
/// </summary>
public class ResourceTreeInfo
{
    /// <summary>
    /// 接口权限树形结构
    /// </summary>
    public ResourceTreeInfo()
    {
    }

    /// <summary>
    /// 接口权限树形结构
    /// </summary>
    /// <param name="label">显示名称</param>
    /// <param name="value">值</param>
    /// <param name="key">键</param>
    public ResourceTreeInfo(string label, string value, string key)
    {
        Label = label;
        Value = value;
        Key = key;
    }

    /// <summary>
    /// Label
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
    /// Key
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 子项
    /// </summary>
    public List<ResourceTreeInfo>? Children { get; set; }

    /// <summary>
    /// 扩展字段
    /// </summary>
    public string Extend { get; set; } = string.Empty;
}