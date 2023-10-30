namespace Athena.Infrastructure.DataAnnotations;

/// <summary>
/// 身份证号验证属性
/// </summary>
public class IdCard15Attribute : DataTypeAttribute
{
    /// <summary>
    /// 15位身份证号
    /// </summary>
    public IdCard15Attribute() : base(DataType.Text)
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
        return str != null && IsIdCard15(str);
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
        return succeed ? ValidationResult.Success : new ValidationResult(ErrorMessage ?? "身份证号不正确");
    }

    /// <summary>  
    /// 验证一代身份证号（15位数）  
    /// [长度为15位的数字；匹配对应省份地址；生日能正确匹配]  
    /// </summary>  
    /// <param name="input">待验证的字符串</param>  
    /// <returns>是否匹配</returns>  
    private static bool IsIdCard15(string input)
    {
        // 验证长度是否为15位
        if (input.Length != 15)
        {
            return false;
        }

        //验证是否可以转换为15位整数  
        if (!long.TryParse(input, out var l) || l.ToString().Length != 15)
        {
            return false;
        }

        //验证省份是否匹配  
        //1~6位为地区代码，其中1、2位数为各省级政府的代码，3、4位数为地、市级政府的代码，5、6位数为县、区级政府代码。  
        const string address =
            "11,12,13,14,15,21,22,23,31,32,33,34,35,36,37,41,42,43,44,45,46,50,51,52,53,54,61,62,63,64,65,71,81,82,91,";
        if (!address.Contains(input.Remove(2) + ","))
        {
            return false;
        }

        //验证生日是否匹配  
        var birthdate = input.Substring(6, 6).Insert(4, "/").Insert(2, "/");
        return DateTime.TryParse(birthdate, out _);
    }
}