namespace Athena.Infrastructure.Excels;

/// <summary>
/// Excel版本
/// </summary>
public enum ExcelVersion
{
    /// <summary>
    /// Excel2003
    /// </summary>
    [Description("XLS")] Xls = 1,

    /// <summary>
    /// Excel2007
    /// </summary>
    [Description("XLSX")] Xlsx = 2
}