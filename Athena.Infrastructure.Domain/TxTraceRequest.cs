namespace Athena.Infrastructure.Domain;

/// <summary>
/// 请求基类
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public class TxTraceRequest<TResponse> : ITxTraceRequest<TResponse>
{
    /// <summary>
    /// 根跟踪Id
    /// </summary>
    public string? RootTraceId { get; set; }
}