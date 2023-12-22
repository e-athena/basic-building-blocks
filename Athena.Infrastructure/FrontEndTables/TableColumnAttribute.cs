namespace Athena.Infrastructure.FrontEndTables;

/// <summary>
/// 表格列特性
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TableColumnAttribute : Attribute
{
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
    /// 列标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 列宽度
    /// </summary>
    public int Width { get; set; } = -1;

    /// <summary>
    /// 提示语
    /// </summary>
    public string? Tooltip { get; set; }

    /// <summary>
    /// 是否必须
    /// <remarks>为true时用户不能关掉</remarks>
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// 固定到左侧或者右侧
    /// <remarks>可选值为 'left' 'right' 'undefined</remarks>
    /// </summary>
    public TableColumnFixed Fixed { get; set; } = TableColumnFixed.Undefined;

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
    /// </summary>
    public TableColumnAlign Align { get; set; } = TableColumnAlign.Left;

    /// <summary>
    /// 是否可排序
    /// </summary>
    public bool Sorter { get; set; }

    /// <summary>
    /// 强制禁用排序，一般用于枚举类型
    /// </summary>
    public bool ForceDisableSorter { get; set; }

    /// <summary>
    /// 是否可筛选
    /// </summary>
    public bool Filters { get; set; }

    /// <summary>
    /// 强制禁用筛选，一般用于枚举类型
    /// </summary>
    public bool ForceDisableFilters { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    public string? ValueType { get; set; }

    /// <summary>
    /// 是否忽略
    /// <remarks>为true时将不会读取</remarks>
    /// </summary>
    public bool Ignore { get; set; }

    /// <summary>
    /// 表格忽略
    /// </summary>
    public bool TableIgnore { get; set; }

    /// <summary>
    /// 是否显示
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
}

/// <summary>
/// 对齐方式
/// </summary>
public enum TableColumnAlign
{
    /// <summary>
    /// 左对齐
    /// </summary>
    Left,

    /// <summary>
    /// 居中对齐
    /// </summary>
    Center,

    /// <summary>
    /// 右对齐
    /// </summary>
    Right
}

/// <summary>
/// 固定方式
/// </summary>
public enum TableColumnFixed
{
    /// <summary>
    /// 左则固定
    /// </summary>
    Left,

    /// <summary>
    /// 右则固定
    /// </summary>
    Right,

    /// <summary>
    /// 不固定
    /// </summary>
    Undefined
}