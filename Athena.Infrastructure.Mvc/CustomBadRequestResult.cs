namespace Athena.Infrastructure.Mvc;

/// <summary>
/// CustomBadRequestResult
/// </summary>
public class CustomBadRequestResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string Message { get; set; } = "Bad Request";

    /// <summary>
    /// 错误信息
    /// </summary>
    public List<Dictionary<string, string[]>>? Errors { get; set; } = new();

    /// <summary>
    /// 状态码
    /// </summary>
    public int StatusCode { get; set; } = 400;

    /// <summary>
    /// TraceId
    /// </summary>
    public string TraceId { get; set; } = Activity.Current?.TraceId.ToString() ?? string.Empty;
}