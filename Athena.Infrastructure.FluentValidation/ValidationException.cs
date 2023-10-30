namespace Athena.Infrastructure.FluentValidation;

/// <summary>
/// 验证异常
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="validationResultModel"></param>
    public ValidationException(ValidationResultModel validationResultModel)
    {
        ValidationResultModel = validationResultModel;
    }

    /// <summary>
    /// 验证结果模型
    /// </summary>
    public ValidationResultModel ValidationResultModel { get; }
}