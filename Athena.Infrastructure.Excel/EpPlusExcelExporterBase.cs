namespace Athena.Infrastructure.Excel;

/// <summary>
/// 
/// </summary>
public class EpPlusExcelExporterBase
{
    /// <summary>
    /// 
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cacheManager"></param>
    protected EpPlusExcelExporterBase(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="excelPackageAction"></param>
    /// <returns></returns>
    protected async Task<FileViewModel> CreateExcelPackageAsync(string fileName,
        Action<ExcelPackage> excelPackageAction)
    {
        var file = new FileViewModel(fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        using var excelPackage = new ExcelPackage();
        excelPackageAction(excelPackage);
        // 保存到缓存中,保存5分钟
        await _cacheManager.SetAsync(file.FileToken, excelPackage.GetAsByteArray(), TimeSpan.FromMinutes(5));
        return file;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="excelPackageAction"></param>
    /// <returns></returns>
    protected Task<byte[]> CreateExcelPackageAsync(Action<ExcelPackage> excelPackageAction)
    {
        using var excelPackage = new ExcelPackage();
        excelPackageAction(excelPackage);
        return Task.FromResult(excelPackage.GetAsByteArray());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sheet"></param>
    /// <param name="headerTexts"></param>
    protected static void AddHeader(ExcelWorksheet sheet, params string[] headerTexts)
    {
        if (headerTexts.Length == 0)
        {
            return;
        }

        for (var i = 0; i < headerTexts.Length; i++)
        {
            AddHeader(sheet, i + 1, headerTexts[i]);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sheet"></param>
    /// <param name="columnIndex"></param>
    /// <param name="headerText"></param>
    private static void AddHeader(ExcelWorksheet sheet, int columnIndex, string headerText)
    {
        sheet.Cells[1, columnIndex].Value = headerText;
        sheet.Cells[1, columnIndex].Style.Font.Bold = true;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sheet"></param>
    /// <param name="startRowIndex"></param>
    /// <param name="items"></param>
    /// <param name="propertySelectors"></param>
    /// <typeparam name="T"></typeparam>
    protected static void AddObject<T>(
        ExcelWorksheet sheet,
        int startRowIndex,
        IList<T> items,
        params Func<T, object>[] propertySelectors)
    {
        if (items.Count == 0 || propertySelectors.Length == 0)
        {
            return;
        }

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            for (var j = 0; j < propertySelectors.Length; j++)
            {
                sheet.Cells[i + startRowIndex, j + 1].Value = propertySelectors[j](item);
            }
        }
    }
}