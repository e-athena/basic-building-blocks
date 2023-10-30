namespace Athena.Infrastructure.FluentValidation;

/// <summary>
/// 验证结果模型
/// </summary>
public class ValidationResultModel
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int StatusCode { get; set; } = (int) HttpStatusCode.BadRequest;

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = "Validation Failed";

    /// <summary>
    /// 错误信息
    /// </summary>
    public List<ValidationError> Errors { get; } = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="validationResult"></param>
    public ValidationResultModel(ValidationResult? validationResult = null)
    {
        if (validationResult == null)
        {
            return;
        }

        Errors = validationResult.Errors
            .Select(error => new ValidationError(error.PropertyName, error.ErrorMessage))
            .ToList();

        Message = Errors.Aggregate("", (current, error) => current + $"{error.Message};");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}