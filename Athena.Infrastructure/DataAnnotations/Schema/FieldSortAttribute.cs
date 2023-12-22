namespace Athena.Infrastructure.DataAnnotations.Schema;

/// <summary>Specifies the data type of the column as a field sort.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class FieldSortAttribute : Attribute
{
    /// <summary>
    /// 排序值
    /// </summary>
    public short Value { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public FieldSortAttribute(short value)
    {
        Value = value;
    }
}