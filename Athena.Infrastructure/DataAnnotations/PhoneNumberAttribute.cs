namespace Athena.Infrastructure.DataAnnotations;

/// <summary>
/// 手机号验证属性
/// </summary>
public class PhoneNumberAttribute : DataTypeAttribute
{
    /// <summary>
    /// 
    /// </summary>
    public PhoneNumberAttribute() : base(DataType.PhoneNumber)
    {
    }

    /// <summary>
    /// 是否验证成功
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override bool IsValid(object? value)
    {
        // 检查是否为空
        if (value == null)
        {
            return true;
        }

        // 转换为字符串
        var str = value.ToString();
        // 验证手机号
        return str != null && Regex.IsMatch(str, @"^1[3456789]\d{9}$");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var succeed = IsValid(value);
        return succeed ? ValidationResult.Success : new ValidationResult(ErrorMessage ?? "手机号格式不正确");
    }
}