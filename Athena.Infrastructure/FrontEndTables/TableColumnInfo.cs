namespace Athena.Infrastructure.FrontEndTables;

/// <summary>
/// 表格列项模型
/// </summary>
public class TableColumnInfo
{
    /// <summary>
    /// 表格忽略
    /// </summary>
    public bool TableIgnore { get; set; }

    /// <summary>
    /// 分组
    /// <remarks>一般用于详情分组显示</remarks>
    /// </summary>
    public string Group { get; set; } = "default";

    /// <summary>
    /// 分组描述
    /// </summary>
    public string? GroupDescription { get; set; }

    /// <summary>
    /// 资源代码
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 列名
    /// </summary>前端表格
    public string DataIndex { get; set; } = null!;

    /// <summary>
    /// 列标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 列宽度
    /// </summary>
    public int? Width { get; set; } = 200;

    /// <summary>
    /// 提示语
    /// </summary>
    public string? Tooltip { get; set; }

    /// <summary>
    /// 在表格中隐藏
    /// </summary>
    public bool HideInTable { get; set; }

    /// <summary>
    /// 在搜索中隐藏
    /// </summary>
    public bool HideInSearch { get; set; }

    /// <summary>
    /// 在详情中隐藏
    /// </summary>
    public bool HideInDescriptions { get; set; }

    /// <summary>
    /// 在表单中隐藏
    /// </summary>
    public bool HideInForm { get; set; }

    /// <summary>
    /// 在设置中隐藏
    /// </summary>
    public bool HideInSetting { get; set; }

    /// <summary>
    /// 是否必须
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// 固定到左侧
    /// </summary>
    public string? Fixed { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; } = 99;

    /// <summary>
    /// 超出宽度显示省略号
    /// </summary>
    public bool Ellipsis { get; set; } = true;

    /// <summary>
    /// 文字对齐方式
    /// <remarks>left，center,right</remarks>
    /// </summary>
    public string Align { get; set; } = "left";

    /// <summary>
    /// 是否可排序
    /// </summary>
    public bool Sorter { get; set; }

    /// <summary>
    /// 是否可筛选
    /// </summary>
    public bool Filters { get; set; }

    /// <summary>
    /// 值类型
    /// </summary>
    public string? ValueType { get; set; }

    /// <summary>
    /// 枚举值
    /// </summary>
    public Dictionary<int, dynamic>? ValueEnum { get; set; }

    /// <summary>
    /// 属性类型
    /// </summary>
    public string? PropertyType { get; set; }

    /// <summary>
    /// 属性名称
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// 枚举选项
    /// </summary>
    public List<dynamic>? EnumOptions { get; set; }
}