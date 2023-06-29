using System.Data;

namespace Athena.Infrastructure.Excels;

/// <summary>
/// 通用的导出接口
/// </summary>
public interface ICommonExcelExporter
{
    /// <summary>
    /// 导出为Excel文件
    /// </summary>
    /// <param name="data">传入的数据集合</param>
    /// <param name="fileName">文件名</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<FileViewModel> ExportToExcelFileAsync<T>(List<T> data, string fileName);

    /// <summary>
    /// 导出为Excel文件
    /// </summary>
    /// <param name="data">传入的数据集合</param>
    /// <param name="fileName">文件名</param>
    /// <param name="sheetName">Sheet名称</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<FileViewModel> ExportToExcelFileAsync<T>(List<T> data, string fileName, string sheetName);

    /// <summary>
    /// 获取Excel文件的内容
    /// </summary>
    /// <param name="data">传入的数据集合</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>二进制</returns>
    Task<byte[]> GetBytesAsync<T>(List<T> data);

    /// <summary>
    /// 获取Excel文件的内容
    /// </summary>
    /// <param name="data">传入的数据集合</param>
    /// <param name="sheetName">Sheet名称</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>二进制</returns>
    Task<byte[]> GetBytesAsync<T>(List<T> data, string sheetName);

    /// <summary>
    /// DataTable导出到Excel
    /// </summary>
    /// <param name="dtSource">源DataTable</param>
    byte[] OutputExcel(DataTable dtSource);

    /// <summary>
    /// DataTable导出到Excel
    /// </summary>
    /// <param name="dtSource">源DataTable</param>
    /// <param name="sheetName">要导入的excel的sheet的名称</param>
    byte[] OutputExcel(DataTable dtSource, string sheetName);

    /// <summary>
    /// DataTable导出到Excel
    /// </summary>
    /// <param name="dtSource">源DataTable</param>
    /// <param name="sheetName">要导入的excel的sheet的名称</param>
    /// <param name="excelVersion">Excel文件类型</param>
    byte[] OutputExcel(DataTable dtSource, string sheetName, ExcelVersion excelVersion);

    /// <summary>
    /// 将excel导入到datatable
    /// </summary>
    /// <param name="filePath">excel路径</param>
    /// <param name="isColumnName">第一行是否是列名</param>
    /// <returns>返回datatable</returns>
    DataTable? ExcelToDataTable(string filePath, bool isColumnName);

    /// <summary>
    /// 将excel导入到datatable
    /// </summary>
    /// <param name="filePath">excel路径</param>
    /// <param name="isColumnName">第一行是否是列名</param>
    /// <param name="sheetName"></param>
    /// <returns>返回datatable</returns>
    DataTable? ExcelToDataTable(string filePath, bool isColumnName, string sheetName);

    /// <summary>
    /// Excel Stream To DataTable
    /// </summary>
    /// <param name="fs"></param>
    /// <param name="isColumnName"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    DataTable? ExcelToDataTable(Stream fs, bool isColumnName, ExcelVersion type);

    /// <summary>
    /// Excel Stream To DataTable
    /// </summary>
    /// <param name="fs"></param>
    /// <param name="isColumnName"></param>
    /// <param name="type"></param>
    /// <param name="sheetName"></param>
    /// <returns></returns>
    DataTable? ExcelToDataTable(Stream fs, bool isColumnName, ExcelVersion type, string sheetName);


    /// <summary>
    /// 获取 Sheets
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    List<string>? GetSheets(string filePath);

    /// <summary>
    /// ExcelToList
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="type"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    List<T> ExcelToList<T>(Stream stream, ExcelVersion type) where T : class, new();

    /// <summary>
    /// ExcelToList
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="type"></param>
    /// <param name="sheetName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    List<T> ExcelToList<T>(Stream stream, ExcelVersion type, string sheetName) where T : class, new();
}