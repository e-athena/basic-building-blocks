namespace Athena.Infrastructure.Attributes;

/// <summary>
/// 枚举状态值特性
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class ValueStatusAttribute : Attribute
{
    /// <summary>
    /// 状态
    /// </summary>
    public ValueEnumStatus Status { get; }

    /// <summary>
    /// 状态值特性
    /// </summary>
    /// <param name="status"></param>
    public ValueStatusAttribute(ValueEnumStatus status = ValueEnumStatus.Default)
    {
        Status = status;
    }
}

/// <summary>
///
/// </summary>
public enum ValueEnumStatus
{
    /// <summary>
    /// Default
    /// </summary>
    Default = 0,

    /// <summary>
    /// Success
    /// </summary>
    Success = 1,

    /// <summary>
    /// Error
    /// </summary>
    Error = 2,

    /// <summary>
    /// Processing
    /// </summary>
    Processing = 3,

    /// <summary>
    /// Warning
    /// </summary>
    Warning = 4
}