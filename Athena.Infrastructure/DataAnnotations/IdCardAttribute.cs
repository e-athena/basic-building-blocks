namespace Athena.Infrastructure.DataAnnotations;

/// <summary>
/// 身份证号验证属性
/// </summary>
public class IdCardAttribute : DataTypeAttribute
{
    /// <summary>
    /// 18位身份证号
    /// </summary>
    public IdCardAttribute() : base(DataType.Text)
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
        return str != null && IsIdCard18(str);
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
    /// 验证二代身份证号（18位数，GB11643-1999标准）  
    /// [长度为18位；前17位为数字，最后一位(校验码)可以为大小写x；匹配对应省份地址；生日能正确匹配；校验码能正确匹配]  
    /// </summary>  
    /// <param name="input">待验证的字符串</param>  
    /// <returns>是否匹配</returns>  
    private static bool IsIdCard18(string input)
    {
        // 验证长度是否为18位
        if (input.Length != 18)
        {
            return false;
        }

        //验证是否可以转换为正确的整数  
        if (!long.TryParse(input.Remove(17), out var l) || l.ToString().Length != 17 ||
            !long.TryParse(input.Replace('x', '0').Replace('X', '0'), out l))
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
        var birthdate = input.Substring(6, 8).Insert(6, "/").Insert(4, "/");
        if (!DateTime.TryParse(birthdate, out _))
        {
            return false;
        }

        //校验码验证  
        //校验码：  
        //（1）十七位数字本体码加权求和公式   
        //S = Sum(Ai * Wi), i = 0, ... , 16 ，先对前17位数字的权求和   
        //Ai:表示第i位置上的身份证号码数字值   
        //Wi:表示第i位置上的加权因子   
        //Wi: 7 9 10 5 8 4 2 1 6 3 7 9 10 5 8 4 2   
        //（2）计算模   
        //Y = mod(S, 11)   
        //（3）通过模得到对应的校验码   
        //Y: 0 1 2 3 4 5 6 7 8 9 10   
        //校验码: 1 0 X 9 8 7 6 5 4 3 2   
        var arrVarifyCode = "1,0,x,9,8,7,6,5,4,3,2".Split(',');
        var wi = "7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2".Split(',');
        var ai = input.Remove(17).ToCharArray();
        var sum = 0;
        for (var i = 0; i < 17; i++)
        {
            sum += int.Parse(wi[i]) * int.Parse(ai[i].ToString());
        }

        Math.DivRem(sum, 11, out var y);
        return arrVarifyCode[y] == input.Substring(17, 1).ToLower();
    }
}