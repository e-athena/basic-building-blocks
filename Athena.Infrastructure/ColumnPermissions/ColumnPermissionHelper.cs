using Athena.Infrastructure.ColumnPermissions.Models;

namespace Athena.Infrastructure.ColumnPermissions;

/// <summary>
///
/// </summary>
public static class ColumnPermissionHelper
{
    /// <summary>
    /// 数据脱敏处理
    /// </summary>
    /// <param name="sources">原始数据</param>
    /// <param name="columnPermissions">列脱敏配置</param>
    /// <typeparam name="TResult">脱敏后的数据</typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static List<TResult> DataMaskHandle<TResult>(
        List<TResult> sources,
        IList<ColumnPermission> columnPermissions
    )
    {
        foreach (var item in sources)
        {
            // 读取属性
            var properties = item!.GetType().GetProperties();
            foreach (var property in properties)
            {
                var propertyName = property.Name;
                // 读取配置
                var single = columnPermissions.FirstOrDefault(p => p.ColumnKey == propertyName);
                if (single == null)
                {
                    // 跳过
                    continue;
                }

                // 读取值
                var propertyValue = property.GetValue(item);
                // 如果值为空，则跳过
                if (propertyValue == null)
                {
                    continue;
                }

                // 如果禁用，则代表没有权限，直接替换处理
                if (!single.Enabled)
                {
                    var propertyType = property.PropertyType;
                    property.SetValue(item,
                        // 如果是字符串，则将数据替换为***， 其他数据类型，使用默认值
                        propertyType == typeof(string) ? "***" : Activator.CreateInstance(propertyType));

                    continue;
                }

                // 脱敏处理
                if (!single.IsEnableDataMask)
                {
                    continue;
                }

                // 根据长度和位置进行替换
                var value = propertyValue.ToString();
                var maskLength = single.MaskLength;
                var maskChar = single.MaskChar;
                var maskPosition = single.MaskPosition;
                var otherLength = value!.Length - maskLength;
                var mask = string.Empty;
                // 如果长度大于等于原始长度，则全部替换
                if (otherLength <= 0)
                {
                    for (var i = 0; i < value.Length; i++)
                    {
                        mask += maskChar;
                    }

                    property.SetValue(item, mask);
                    continue;
                }

                // 如果长度小于原始长度，则根据位置进行替换
                for (var i = 0; i < maskLength; i++)
                {
                    mask += maskChar;
                }

                // 根据位置进行替换
                switch (maskPosition)
                {
                    case MaskPosition.Front:
                        property.SetValue(item, mask + value[otherLength..]);
                        break;
                    case MaskPosition.Middle:
                        var middle = otherLength / 2;
                        // 将中间部分替换为掩码字符，长度要等于掩码长度
                        property.SetValue(item, value[..middle] + mask + value[(maskLength + middle)..]);
                        break;
                    case MaskPosition.Back:
                        property.SetValue(item, value[..otherLength] + mask);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        return sources;
    }
}