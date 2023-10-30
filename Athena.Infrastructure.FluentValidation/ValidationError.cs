namespace Athena.Infrastructure.FluentValidation;

/// <summary>
/// 验证错误
/// </summary>
public class ValidationError
{
    /// <summary>
    /// 字段
    /// </summary>
    public string? Field { get; }

    /// <summary>
    /// 信息
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="field"></param>
    /// <param name="message"></param>
    public ValidationError(string field, string message)
    {
        Field = field != string.Empty ? field : null;
        Message = message;
    }
}