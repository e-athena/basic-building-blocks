namespace Athena.Infrastructure.Enums;

/// <summary>
/// 状态
///     启用/禁用
/// </summary>
public enum Status
{
    /// <summary>
    /// 启用
    /// </summary>
    [Description("启用")] Enabled = 1,

    /// <summary>
    /// 禁用
    /// </summary>
    [Description("禁用")] Disabled = 2,
}