namespace Athena.Infrastructure.ColumnPermissions.Models;

/// <summary>
/// 列权限
/// </summary>
public class ColumnPermission
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 列Key
    /// </summary>
    public string ColumnKey { get; set; } = null!;

    /// <summary>
    /// 启用数据脱敏
    /// </summary>
    public bool IsEnableDataMask { get; set; }

    /// <summary>
    /// 掩码长度
    /// </summary>
    public int MaskLength { get; set; } = 4;

    /// <summary>
    /// 掩码位置，1、前面，2、中间，3、后面
    /// </summary>
    public MaskPosition MaskPosition { get; set; } = MaskPosition.Middle;

    /// <summary>
    /// 掩码字符
    /// </summary>
    public string MaskChar { get; set; } = "*";
}

/// <summary>
/// 掩码位置
/// </summary>
public enum MaskPosition
{
    /// <summary>
    /// 前面
    /// </summary>
    Front = 1,

    /// <summary>
    /// 中间
    /// </summary>
    Middle = 2,

    /// <summary>
    /// 后面
    /// </summary>
    Back = 3
}