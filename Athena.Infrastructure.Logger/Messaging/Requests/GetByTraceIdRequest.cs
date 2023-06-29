namespace Athena.Infrastructure.Logger.Messaging.Requests;

/// <summary>
/// 读取日志详情
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class GetByTraceIdRequest : RequestBase
{
    /// <summary>
    /// TraceId
    /// </summary>
    public string TraceId { get; set; } = null!;
}