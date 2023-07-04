namespace Athena.Infrastructure.EventTracking.Enums;

/// <summary>
/// 执行状态
/// <remarks>0、未执行，1、执行成功，2、执行失败</remarks>
/// </summary>
public enum TrackStatus
{
    /// <summary>
    /// 未执行
    /// </summary>
    [Description("未执行")] NotExecute = 0,
    
    /// <summary>
    /// 执行中
    /// </summary>
    [Description("执行中")] Executing = 1,

    /// <summary>
    /// 执行成功
    /// </summary>
    [Description("执行成功")] Success = 2,

    /// <summary>
    /// 执行失败
    /// </summary>
    [Description("执行失败")] Fail = 3
}