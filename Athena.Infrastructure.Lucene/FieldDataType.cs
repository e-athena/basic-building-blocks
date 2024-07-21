namespace Athena.Infrastructure.Lucene;

public enum FieldDataType
{
    /// <summary>
    /// Html文本
    /// </summary>
    Html,
    /// <summary>
    /// JSON文本
    /// </summary>
    Json,
    /// <summary>
    /// 纯文本
    /// </summary>
    Text,
    /// <summary>
    /// Xml文本
    /// </summary>
    Xml,
    /// <summary>
    /// Csv文本
    /// </summary>
    Csv,
    /// <summary>
    /// 年份yyyy
    /// </summary>
    DateYear,
    /// <summary>
    /// 时间
    /// </summary>
    DateTime,
    /// <summary>
    /// 数字
    /// </summary>
    Int32,
    /// <summary>
    /// 数字
    /// </summary>
    Int64,
    /// <summary>
    /// 小数
    /// </summary>
    Double,
    /// <summary>
    /// 字典文本
    /// </summary>
    Dic,
    /// <summary>
    /// 单词文本
    /// </summary>
    String,
    /// <summary>
    /// 维度统计
    /// </summary>
    Facet,
    /// <summary>
    /// 地理信息坐标
    /// </summary>
    Geo
}