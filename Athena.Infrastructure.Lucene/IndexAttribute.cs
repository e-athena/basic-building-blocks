namespace Athena.Infrastructure.Lucene;

/// <summary>
/// 索引属性
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class IndexAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public IndexAttribute()
    {
        IsStore = Field.Store.YES;
        FieldType = FieldDataType.Text;
    }

    /// <summary>
    /// 名称
    /// </summary>
    public string FieldName { get; set; }
    /// <summary>
    /// 是否存储
    /// </summary>
    public Field.Store IsStore { get; set; }
    /// <summary>
    /// 数据格式
    /// </summary>
    public FieldDataType FieldType { get; set; }

}