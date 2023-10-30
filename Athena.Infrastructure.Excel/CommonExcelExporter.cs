using Athena.Infrastructure.DataAnnotations;

namespace Athena.Infrastructure.Excel;

/// <summary>
/// 
/// </summary>
public class CommonExcelExporter : EpPlusExcelExporterBase, ICommonExcelExporter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cacheManager"></param>
    public CommonExcelExporter(ICacheManager cacheManager) : base(cacheManager)
    {
    }

    /// <summary>
    /// 导出
    /// </summary>
    /// <param name="data"></param>
    /// <param name="fileName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task<FileViewModel> ExportToExcelFileAsync<T>(List<T> data, string fileName)
    {
        return ExportToExcelFileAsync(data, fileName, "Sheet1");
    }

    /// <summary>
    /// 导出
    /// </summary>
    /// <param name="data"></param>
    /// <param name="fileName"></param>
    /// <param name="sheetName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<FileViewModel> ExportToExcelFileAsync<T>(List<T> data, string fileName, string sheetName)
    {
        var fileExportName = $"{fileName}[{DateTime.Now:yyyyMMddHHmmss}].xlsx";
        var excel = await CreateExcelPackageAsync(fileExportName, excelPackage =>
        {
            var sheet = excelPackage.Workbook.Worksheets.Add(sheetName);
            sheet.OutLineApplyStyle = true;
            AddHeader(sheet, GetExportHeader(typeof(T)));
            AddObject(sheet, 2, data, GetExportFunc<T>(typeof(T)));
        });
        return excel;
    }

    /// <summary>
    /// 获取Excel文件的内容
    /// </summary>
    /// <param name="data"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task<byte[]> GetBytesAsync<T>(List<T> data)
    {
        return GetBytesAsync(data, "Sheet1");
    }

    /// <summary>
    /// 获取Excel文件的内容
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sheetName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<byte[]> GetBytesAsync<T>(List<T> data, string sheetName)
    {
        var excel = await CreateExcelPackageAsync(excelPackage =>
        {
            var sheet = excelPackage.Workbook.Worksheets.Add(sheetName);
            sheet.OutLineApplyStyle = true;
            AddHeader(sheet, GetExportHeader(typeof(T)));
            AddObject(sheet, 2, data, GetExportFunc<T>(typeof(T)));
        });
        return excel;
    }

    /// <summary>
    /// DataTable导出到Excel
    /// </summary>
    /// <param name="dtSource">源DataTable</param>
    public byte[] OutputExcel(DataTable dtSource)
    {
        return OutputExcel(dtSource, "Sheet1");
    }

    /// <summary>
    /// DataTable导出到Excel
    /// </summary>
    /// <param name="dtSource">源DataTable</param>
    /// <param name="sheetName">要导入的excel的sheet的名称</param>
    public byte[] OutputExcel(DataTable dtSource, string sheetName)
    {
        return OutputExcel(dtSource, sheetName, ExcelVersion.Xlsx);
    }

    /// <summary>
    /// DataTable导出到Excel
    /// </summary>
    /// <param name="dtSource">源DataTable</param>
    /// <param name="sheetName">要导入的excel的sheet的名称</param>
    /// <param name="excelVersion">Excel文件类型</param>
    public byte[] OutputExcel(DataTable dtSource, string sheetName, ExcelVersion excelVersion)
    {
        IWorkbook workbook = excelVersion switch
        {
            ExcelVersion.Xls => new HSSFWorkbook(),
            ExcelVersion.Xlsx => new XSSFWorkbook(),
            _ => throw new Exception("不受支持的文件类型")
        };

        var sheet = workbook.CreateSheet(sheetName);

        var dateStyle = workbook.CreateCellStyle();
        var format = workbook.CreateDataFormat();
        dateStyle.DataFormat = format.GetFormat("yyyy-mm-dd");

        //取得列宽
        var arrColWidth = new int[dtSource.Columns.Count];
        foreach (DataColumn item in dtSource.Columns)
        {
            arrColWidth[item.Ordinal] = Encoding.GetEncoding(65001).GetBytes(item.ColumnName).Length;
        }

        for (var i = 0; i < dtSource.Rows.Count; i++)
        {
            for (var j = 0; j < dtSource.Columns.Count; j++)
            {
                var intTemp = Encoding.GetEncoding(65001).GetBytes(dtSource.Rows[i][j].ToString()!).Length;
                if (intTemp > arrColWidth[j])
                {
                    arrColWidth[j] = intTemp;
                }
            }
        }

        var rowIndex = 0;

        foreach (DataRow row in dtSource.Rows)
        {
            #region 新建表，填充表头，填充列头，样式

            if (rowIndex is 65535 or 0)
            {
                if (rowIndex != 0)
                {
                    sheet = workbook.CreateSheet();
                }

                #region 列头及样式

                var headerRow = sheet.CreateRow(0);

                var headStyle = workbook.CreateCellStyle();
                headStyle.Alignment = HorizontalAlignment.Center;

                var font = workbook.CreateFont();
                font.FontHeightInPoints = 10;
                font.Boldweight = 700;
                headStyle.SetFont(font);


                foreach (DataColumn column in dtSource.Columns)
                {
                    headerRow.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
                    headerRow.GetCell(column.Ordinal).CellStyle = headStyle;

                    //设置列宽
                    sheet.SetColumnWidth(column.Ordinal, (arrColWidth[column.Ordinal] + 1) * 256);
                }

                #endregion

                rowIndex = 1;
            }

            #endregion

            #region 填充内容

            var dataRow = sheet.CreateRow(rowIndex);
            foreach (DataColumn column in dtSource.Columns)
            {
                var newCell = dataRow.CreateCell(column.Ordinal);

                var drValue = row[column].ToString();

                switch (column.DataType.ToString())
                {
                    case "System.String": //字符串类型
                        newCell.SetCellValue(drValue);
                        break;
                    case "System.DateTime": //日期类型
                        DateTime.TryParse(drValue, out var dateV);
                        newCell.SetCellValue(dateV);

                        newCell.CellStyle = dateStyle; //格式化显示
                        break;
                    case "System.Boolean": //布尔型
                        bool.TryParse(drValue, out var boolV);
                        newCell.SetCellValue(boolV);
                        break;
                    case "System.Int16": //整型
                    case "System.Int32":
                    case "System.Int64":
                    case "System.Byte":
                        int.TryParse(drValue, out var intV);
                        newCell.SetCellValue(intV);
                        break;
                    case "System.Decimal": //浮点型
                    case "System.Double":
                        double.TryParse(drValue, out var doubleV);
                        newCell.SetCellValue(doubleV);
                        break;
                    case "System.DBNull": //空值处理
                        newCell.SetCellValue("");
                        break;
                    default:
                        newCell.SetCellValue("");
                        break;
                }
            }

            #endregion

            rowIndex++;
        }

        using var ms = new MemoryStream();
        workbook.Write(ms);
        var buffer = ms.GetBuffer();
        ms.Close();
        return buffer;
    }

    /// <summary>
    /// 将excel导入到datatable
    /// </summary>
    /// <param name="filePath">excel路径</param>
    /// <param name="isColumnName">第一行是否是列名</param>
    /// <returns>返回datatable</returns>
    public DataTable? ExcelToDataTable(string filePath, bool isColumnName)
    {
        return ExcelToDataTable(filePath, isColumnName, "");
    }

    /// <summary>
    /// 将excel导入到datatable
    /// </summary>
    /// <param name="filePath">excel路径</param>
    /// <param name="isColumnName">第一行是否是列名</param>
    /// <param name="sheetName"></param>
    /// <returns>返回datatable</returns>
    public DataTable? ExcelToDataTable(string filePath, bool isColumnName, string sheetName)
    {
        DataTable? dataTable = null;
        FileStream? fs = null;
        IWorkbook? workbook = null;
        var startRow = 0;
        try
        {
            using (fs = File.OpenRead(filePath))
            {
                // 2007版本
                if (filePath.IndexOf(".xlsx", StringComparison.Ordinal) > 0)
                    workbook = new XSSFWorkbook(fs);
                // 2003版本
                else if (filePath.IndexOf(".xls", StringComparison.Ordinal) > 0)
                    workbook = new HSSFWorkbook(fs);

                if (workbook != null)
                {
                    var sheet = string.IsNullOrEmpty(sheetName) ? workbook.GetSheetAt(0) : workbook.GetSheet(sheetName);
                    dataTable = new DataTable();
                    if (sheet != null)
                    {
                        var rowCount = sheet.LastRowNum; //总行数
                        if (rowCount > 0)
                        {
                            var firstRow = sheet.GetRow(0); //第一行
                            int cellCount = firstRow.LastCellNum; //列数

                            //构建datatable的列
                            DataColumn column;
                            ICell cell;
                            if (isColumnName)
                            {
                                startRow = 1; //如果第一行是列名，则从第二行开始读取
                                for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                {
                                    cell = firstRow.GetCell(i);
                                    if (cell?.StringCellValue != null)
                                    {
                                        column = new DataColumn(cell.StringCellValue);
                                        dataTable.Columns.Add(column);
                                    }
                                }
                            }
                            else
                            {
                                for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                {
                                    column = new DataColumn("column" + (i + 1));
                                    dataTable.Columns.Add(column);
                                }
                            }

                            //填充行
                            for (var i = startRow; i <= rowCount; ++i)
                            {
                                var row = sheet.GetRow(i);
                                if (row == null) continue;

                                var dataRow = dataTable.NewRow();
                                for (int j = row.FirstCellNum; j < cellCount; ++j)
                                {
                                    cell = row.GetCell(j);
                                    if (cell == null)
                                    {
                                        dataRow[j] = "";
                                    }
                                    else
                                    {
                                        //CellType(Unknown = -1,Numeric = 0,String = 1,Formula = 2,Blank = 3,Boolean = 4,Error = 5,)
                                        switch (cell.CellType)
                                        {
                                            case CellType.Blank:
                                                dataRow[j] = "";
                                                break;
                                            case CellType.Numeric:
                                                var format = cell.CellStyle.DataFormat;
                                                //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理
                                                if (format is 14 or 31 or 57 or 58)
                                                    dataRow[j] = cell.DateCellValue;
                                                else
                                                    dataRow[j] = cell.NumericCellValue;
                                                break;
                                            case CellType.String:
                                                dataRow[j] = cell.StringCellValue;
                                                break;
                                        }
                                    }
                                }

                                dataTable.Rows.Add(dataRow);
                            }
                        }
                    }
                }
            }

            return dataTable;
        }
        catch (Exception)
        {
            fs?.Close();
            return null;
        }
    }

    /// <summary>
    /// Excel Stream To DataTable
    /// </summary>
    /// <param name="fs"></param>
    /// <param name="isColumnName"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public DataTable? ExcelToDataTable(Stream fs, bool isColumnName, ExcelVersion type)
    {
        return ExcelToDataTable(fs, isColumnName, type, "Sheet1");
    }

    /// <summary>
    /// Excel Stream To DataTable
    /// </summary>
    /// <param name="fs"></param>
    /// <param name="isColumnName"></param>
    /// <param name="type"></param>
    /// <param name="sheetName"></param>
    /// <returns></returns>
    public DataTable? ExcelToDataTable(Stream fs, bool isColumnName, ExcelVersion type, string sheetName)
    {
        DataTable? dataTable = null;
        IWorkbook? workbook = null;
        var startRow = 0;
        try
        {
            // 2007版本
            if (type == ExcelVersion.Xlsx)
                workbook = new XSSFWorkbook(fs);
            // 2003版本
            else if (type == ExcelVersion.Xls)
                workbook = new HSSFWorkbook(fs);

            if (workbook != null)
            {
                //sheet = workbook.GetSheetAt(sheetIndex);//读取第一个sheet，当然也可以循环读取每个sheet
                var sheet = workbook.GetSheet(sheetName);
                dataTable = new DataTable();
                if (sheet != null)
                {
                    var rowCount = sheet.LastRowNum; //总行数
                    if (rowCount > 0)
                    {
                        var firstRow = sheet.GetRow(0); //第一行
                        int cellCount = firstRow.LastCellNum; //列数

                        //构建datatable的列
                        DataColumn column;
                        ICell cell;
                        if (isColumnName)
                        {
                            startRow = 1; //如果第一行是列名，则从第二行开始读取
                            for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                            {
                                cell = firstRow.GetCell(i);
                                if (cell?.StringCellValue != null)
                                {
                                    column = new DataColumn(cell.StringCellValue);
                                    dataTable.Columns.Add(column);
                                }
                            }
                        }
                        else
                        {
                            for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                            {
                                column = new DataColumn("column" + (i + 1));
                                dataTable.Columns.Add(column);
                            }
                        }

                        //填充行
                        for (var i = startRow; i <= rowCount; ++i)
                        {
                            var row = sheet.GetRow(i);
                            if (row == null) continue;

                            var dataRow = dataTable.NewRow();
                            for (int j = row.FirstCellNum; j < cellCount; ++j)
                            {
                                cell = row.GetCell(j);
                                if (cell == null)
                                {
                                    dataRow[j] = "";
                                }
                                else
                                {
                                    //CellType(Unknown = -1,Numeric = 0,String = 1,Formula = 2,Blank = 3,Boolean = 4,Error = 5,)
                                    switch (cell.CellType)
                                    {
                                        case CellType.Blank:
                                            dataRow[j] = "";
                                            break;
                                        case CellType.Numeric:
                                            var format = cell.CellStyle.DataFormat;
                                            //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理
                                            if (format is 14 or 31 or 57 or 58)
                                                dataRow[j] = cell.DateCellValue;
                                            else
                                                dataRow[j] = cell.NumericCellValue;
                                            break;
                                        case CellType.String:
                                            dataRow[j] = cell.StringCellValue;
                                            break;
                                    }
                                }
                            }

                            dataTable.Rows.Add(dataRow);
                        }
                    }
                }
            }

            return dataTable;
        }
        catch (Exception)
        {
            fs.Close();
            return null;
        }
    }


    /// <summary>
    /// 获取 Sheets
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public List<string>? GetSheets(string filePath)
    {
        List<string>? sheetNames = null;
        FileStream? fs = null;
        IWorkbook? workbook = null;
        try
        {
            using (fs = File.OpenRead(filePath))
            {
                // 2007版本
                if (filePath.IndexOf(".xlsx", StringComparison.Ordinal) > 0)
                {
                    workbook = new XSSFWorkbook(fs);
                }
                // 2003版本
                else if (filePath.IndexOf(".xls", StringComparison.Ordinal) > 0)
                {
                    workbook = new HSSFWorkbook(fs);
                }
            }

            if (workbook != null)
            {
                sheetNames = new List<string>();
                var sheetCount = workbook.NumberOfSheets; //获取表的数量
                for (var i = 0; i < sheetCount; i++)
                {
                    sheetNames.Add(workbook.GetSheetName(i));
                }
            }
        }
        catch (Exception)
        {
            fs?.Close();
            return null;
        }

        return sheetNames;
    }


    /// <summary>
    /// ExcelToList
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="type"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public List<T> ExcelToList<T>(Stream stream, ExcelVersion type)
        where T : class, new()
    {
        return ExcelToList<T>(stream, type, "Sheet1");
    }

    /// <summary>
    /// ExcelToList
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="type"></param>
    /// <param name="sheetName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public List<T> ExcelToList<T>(Stream stream, ExcelVersion type, string sheetName)
        where T : class, new()
    {
        var dict = GetDictionaryMapping(typeof(T));
        var containerCols = GetRequiredColumns(typeof(T));
        if (containerCols.Count == 0)
        {
            throw FriendlyException.Of("至少需要设置一列为必填项，请联系管理员");
        }

        var dataTable = ExcelToDataTable(stream, type, containerCols, dict, sheetName);
        return DataTableToList<T>(dataTable);
    }


    #region 私有方法

    /// <summary>
    /// 获取导出内容
    /// </summary>
    /// <param name="type"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private Func<T, object>[] GetExportFunc<T>(Type type)
    {
        var funcList = new List<Func<T, object>>();

        foreach (var property in type.GetProperties())
        {
            var objs = property.GetCustomAttributes(typeof(DescriptionAttribute), true);
            // 标记了DescriptionAttribute的才导出
            if (objs.Length == 0)
            {
                continue;
            }

            object? Func(T c) => type.GetProperty(property.Name)?.GetValue(c);
            funcList.Add(Func!);
        }

        return funcList.ToArray();
    }

    /// <summary>
    /// 获取导出头
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private string[] GetExportHeader(Type type)
    {
        var list = new List<string>();
        foreach (var property in type.GetProperties())
        {
            var objs = property.GetCustomAttributes(typeof(DescriptionAttribute), true);
            // 标记了DescriptionAttribute的才导出
            if (objs.Length == 0)
            {
                continue;
            }

            var name = ((DescriptionAttribute) objs[0]).Description;
            list.Add(name);
        }

        return list.ToArray();
    }

    /// <summary>
    /// 读取映射值字典
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static Dictionary<string, string> GetDictionaryMapping(Type type)
    {
        var dict = new Dictionary<string, string>();
        foreach (var property in type.GetProperties())
        {
            var objs = property.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objs.Length > 0)
            {
                var name = ((DescriptionAttribute) objs[0]).Description;
                dict.Add(name, property.Name);
            }
            else
            {
                dict.Add(property.Name, property.Name);
            }
        }

        return dict;
    }

    /// <summary>
    /// 读取必须包含的列
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static List<string> GetRequiredColumns(Type type)
    {
        var list = new List<string>();
        foreach (var property in type.GetProperties())
        {
            var objs = property.GetCustomAttributes(typeof(DescriptionAttribute), false);
            var reqObjs = property.GetCustomAttributes(typeof(RequiredAttribute), false);
            // if (objs.Length <= 0 || reqObjs.Length <= 0)
            if (reqObjs.Length <= 0)
            {
                continue;
            }

            var name = objs.Length > 0 ? ((DescriptionAttribute) objs[0]).Description : property.Name;
            list.Add(name);
        }

        return list;
    }

    /// <summary>
    /// Excel Stream To DataTable
    /// </summary>
    /// <param name="stream">文件流</param>
    /// <param name="type">Excel文件类型</param>
    /// <param name="containerCols">必须包含的列</param>
    /// <param name="dict"></param>
    /// <param name="sheetName"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    private static DataTable ExcelToDataTable(
        Stream stream,
        ExcelVersion type,
        ICollection<string> containerCols,
        Dictionary<string, string> dict,
        string sheetName = "Sheet1")
    {
        try
        {
            IWorkbook workbook = type switch
            {
                ExcelVersion.Xls => new HSSFWorkbook(stream),
                ExcelVersion.Xlsx => new XSSFWorkbook(stream),
                _ => throw FriendlyException.Of("不支持的Excel文件类型")
            };

            if (workbook == null)
            {
                throw FriendlyException.Of("不支持的Excel文件类型");
            }

            var sheet = workbook.GetSheet(sheetName);
            if (sheet == null)
            {
                throw FriendlyException.Of("读取不到指定的Sheet：" + sheetName);
            }

            var rowCount = sheet.LastRowNum; //总行数
            if (rowCount == 0)
            {
                throw FriendlyException.Of("Excel的数据为空，请检查！");
            }

            var dataTable = new DataTable();

            IRow firstRow; //第一行
            int cellCount; //列数

            // 处理前面有空行的问题
            var index = 0;
            int startRow;
            while (true)
            {
                firstRow = sheet.GetRow(index);
                if (firstRow == null)
                {
                    index++;
                    continue;
                }

                cellCount = firstRow.LastCellNum;

                var hasCount = 0;
                for (int i = firstRow.FirstCellNum; i < cellCount; i++)
                {
                    var c = firstRow.GetCell(i);
                    if (c == null)
                    {
                        continue;
                    }

                    string str;
                    switch (c.CellType)
                    {
                        case CellType.String:
                            str = c.StringCellValue;
                            break;
                        default:
                            continue;
                    }


                    if (containerCols.Any(p => p == str))
                    {
                        hasCount++;
                    }
                }

                // 如果Excel拥有的列包含传进来的列，则代表列不缺失
                if (hasCount == containerCols.Count)
                {
                    // 数据开始行是标题的下一行
                    startRow = index + 1;
                    break;
                }

                index++;

                // 如果超过最大行，则报错
                if (index >= rowCount)
                {
                    throw FriendlyException.Of("缺少数据列,请检查.");
                }
            }

            //构建datatable的列
            ICell cell;

            // 处理列数据
            for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
            {
                cell = firstRow.GetCell(i);
                if (cell == null)
                {
                    continue;
                }

                string str;
                switch (cell.CellType)
                {
                    case CellType.Blank:
                        str = "";
                        break;
                    case CellType.String:
                        str = cell.StringCellValue;
                        break;
                    case CellType.Numeric:
                        str = DateUtil.IsCellDateFormatted(cell)
                            ? cell.DateCellValue.ToString(CultureInfo.InvariantCulture)
                            : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
                        break;
                    case CellType.Formula:
                        str = cell.CellFormula;
                        break;
                    default:
                        continue;
                }

                var column = new DataColumn(str);
                var (_, value) = dict.FirstOrDefault(p => p.Key == column.ToString());
                if (!string.IsNullOrEmpty(value))
                {
                    dataTable.Columns.Add(value);
                }
            }

            //填充行
            for (var i = startRow; i <= rowCount; ++i)
            {
                var row = sheet.GetRow(i);
                if (row == null || row.FirstCellNum == -1)
                {
                    continue;
                }

                var dataRow = dataTable.NewRow();
                var emptyCount = 0;
                for (int j = row.FirstCellNum; j < dataTable.Columns.Count; ++j)
                {
                    var colName = dataTable.Columns[j].ColumnName;
                    var d = dict.FirstOrDefault(p => p.Value == colName);
                    var hasColName = containerCols.Any(p => p == d.Key);
                    cell = row.GetCell(j);
                    if (cell == null)
                    {
                        if (hasColName)
                        {
                            emptyCount++;
                        }

                        continue;
                    }

                    switch (cell.CellType)
                    {
                        case CellType.Blank:
                            dataRow[j] = "";

                            if (hasColName)
                            {
                                emptyCount++;
                            }

                            break;
                        case CellType.Numeric:
                            //NPOI中数字和日期都是NUMERIC类型的，这里对其进行判断是否是日期类型
                            if (DateUtil.IsCellDateFormatted(cell)) //日期类型
                            {
                                dataRow[j] = cell.DateCellValue;
                            }
                            else //其他数字类型
                            {
                                dataRow[j] = cell.NumericCellValue;
                            }

                            break;
                        case CellType.String:
                            dataRow[j] = cell.StringCellValue;
                            if (cell.StringCellValue.Equals("") && hasColName)
                            {
                                emptyCount++;
                            }

                            break;
                        case CellType.Unknown:
                            break;
                        case CellType.Formula:
                            break;
                        case CellType.Boolean:
                            break;
                        case CellType.Error:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                // 必填项都不为空则添加进去
                if (emptyCount == 0)
                {
                    dataTable.Rows.Add(dataRow);
                }
            }

            return dataTable;
        }
        catch (FriendlyException)
        {
            throw;
        }
        // 其他错误
        catch (Exception ex)
        {
            throw FriendlyException.Of("导入模板格式错误，请检查.", ex.Message);
        }
        finally
        {
            stream.Close();
        }
    }


    /// <summary>
    /// DataTableToList
    /// </summary>
    /// <param name="dt"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static List<T> DataTableToList<T>(DataTable dt) where T : class, new()
    {
        var type = typeof(T);
        var propertyInfos = type.GetProperties();
        var list = new List<T>();
        foreach (DataRow item in dt.Rows)
        {
            var obj = new T();
            foreach (var info in propertyInfos)
            {
                var typeName = info.Name;
                var desc = info.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .FirstOrDefault() as DescriptionAttribute;
                if (!dt.Columns.Contains(typeName))
                {
                    continue;
                }

                if (!info.CanWrite)
                {
                    continue;
                }

                var valueObj = item[typeName];
                if (valueObj == DBNull.Value || string.IsNullOrEmpty(valueObj.ToString()))
                {
                    continue;
                }

                // 去掉空格
                var value = valueObj.ToString()?.Replace(" ", "");

                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                #region 数据校验与转换

                if (info.PropertyType == typeof(string))
                {
                    info.SetValue(obj, value, null);

                    // 校验其他特性
                    // 邮箱
                    if (info.GetCustomAttributes(typeof(EmailAddressAttribute), false)
                            .FirstOrDefault() is EmailAddressAttribute)
                    {
                        if (!VerificationHelper.IsEmail(value))
                        {
                            throw FriendlyException.Of($"[{desc?.Description ?? typeName}]列只支持邮箱地址，请检查！");
                        }
                    }

                    // 手机号
                    if (info.GetCustomAttributes(typeof(PhoneNumberAttribute), false)
                            .FirstOrDefault() is PhoneNumberAttribute)
                    {
                        if (!VerificationHelper.IsPhoneNumber(value))
                        {
                            throw FriendlyException.Of($"[{desc?.Description ?? typeName}]列只支持11位手机号，请检查！");
                        }
                    }
                    // 其他待扩展
                }
                else if (info.PropertyType == typeof(decimal))
                {
                    if (decimal.TryParse(value, out var v))
                    {
                        info.SetValue(obj, v, null);
                    }
                    else
                    {
                        throw FriendlyException.Of($"[{desc?.Description ?? typeName}]列只支持数值类型，请检查！");
                    }
                }
                else if (info.PropertyType == typeof(decimal?))
                {
                    if (decimal.TryParse(value, out var v))
                    {
                        info.SetValue(obj, v, null);
                    }
                    else
                    {
                        throw FriendlyException.Of($"[{desc?.Description ?? typeName}]列只支持数值类型，请检查！");
                    }
                }
                else if (info.PropertyType == typeof(int))
                {
                    if (int.TryParse(value, out var v))
                    {
                        info.SetValue(obj, v, null);
                    }
                    else
                    {
                        throw FriendlyException.Of($"[{desc?.Description ?? typeName}]列只支持数值类型，请检查！");
                    }
                }
                else if (info.PropertyType == typeof(int?))
                {
                    if (int.TryParse(value, out var v))
                    {
                        info.SetValue(obj, v, null);
                    }
                    else
                    {
                        throw FriendlyException.Of($"[{desc?.Description ?? typeName}]列只支持数值类型，请检查！");
                    }
                }
                else if (info.PropertyType == typeof(double))
                {
                    if (double.TryParse(value, out var v))
                    {
                        info.SetValue(obj, v, null);
                    }
                    else
                    {
                        throw FriendlyException.Of($"[{desc?.Description ?? typeName}]列只支持数值类型，请检查！");
                    }
                }
                else if (info.PropertyType == typeof(double?))
                {
                    if (double.TryParse(value, out var v))
                    {
                        info.SetValue(obj, v, null);
                    }
                    else
                    {
                        throw FriendlyException.Of($"[{desc?.Description ?? typeName}]列只支持数值类型，请检查！");
                    }
                }
                else if (info.PropertyType == typeof(bool))
                {
                    if (value is "是" or "否")
                    {
                        info.SetValue(obj, value == "是", null);
                    }
                    else
                    {
                        throw FriendlyException.Of($"[{desc?.Description ?? typeName}]列只支持布尔类型（是/否），请检查！");
                    }
                }
                else if (info.PropertyType == typeof(bool?))
                {
                    if (value is "是" or "否")
                    {
                        info.SetValue(obj, value == "是", null);
                    }
                    else
                    {
                        throw FriendlyException.Of($"[{desc?.Description ?? typeName}]列只支持布尔类型（是/否），请检查！");
                    }
                }
                else if (info.PropertyType == typeof(DateTime))
                {
                    if (DateTime.TryParse(value, out var v))
                    {
                        info.SetValue(obj, v, null);
                    }
                    else
                    {
                        throw FriendlyException.Of($"[{desc?.Description ?? typeName}]列只支持日期时间类型，请检查！");
                    }
                }
                else if (info.PropertyType == typeof(DateTime?))
                {
                    if (DateTime.TryParse(value, out var v))
                    {
                        info.SetValue(obj, v, null);
                    }
                    else
                    {
                        throw FriendlyException.Of($"[{desc?.Description ?? typeName}]列只支持日期时间类型，请检查！");
                    }
                }
                else
                {
                    throw FriendlyException.Of($"未支持[{info.PropertyType.Name}]类型，请联系管理员");
                }

                #endregion
            }

            list.Add(obj);
        }

        return list;
    }

    #endregion
}