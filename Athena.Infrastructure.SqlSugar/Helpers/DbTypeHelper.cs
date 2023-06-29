using System.Text.RegularExpressions;

namespace Athena.Infrastructure.SqlSugar.Helpers;

/// <summary>
/// 数据库类型帮助类
/// </summary>
public static class DbTypeHelper
{
    /// <summary>
    /// 读取数据库类型
    /// </summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns></returns>
    public static DbType GetDataType(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }
        // 使用正则表达式判断数据库类型

        if (Regex.IsMatch(connectionString, @"Data Source=\S+\.db;|\b(SQLite)\b"))
        {
            return DbType.Sqlite;
        }

        if (Regex.IsMatch(connectionString, @"\b(MySQL|MariaDB)\b", RegexOptions.IgnoreCase))
        {
            return DbType.MySql;
        }

        if (Regex.IsMatch(connectionString, @"\b(Data Source|Server)\b", RegexOptions.IgnoreCase))
        {
            return DbType.SqlServer;
        }

        if (Regex.IsMatch(connectionString,
                @"Server=\S+;Port=\d+;Database=\S+;User Id=\S+;Password=\S+;|\b(PostgreSQL)\b"))
        {
            return DbType.PostgreSQL;
        }

        throw new Exception("不支持的数据库类型");
    }

    /// <summary>
    /// 读取数据库类型
    /// </summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns></returns>
    public static (DbType dataType, string connectionString) GetDataTypeAndConnectionString(
        string connectionString)
    {
        // 使用逗号分隔连接字符串，第一个为数据库类型，第二个为数据库连接字符串
        var array = connectionString.Split(',');
        var res = array.Length == 1
            ? (GetDataType(connectionString), connectionString)
            : (GetDataTypeByName(array[0]), array[1]);
        return res;
    }

    /// <summary>
    /// 根据名称获取数据库类型
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static DbType GetDataTypeByName(string name)
    {
        switch (name.ToLower())
        {
            case "mysql":
            case "mariadb":
                return DbType.MySql;
            case "sqlserver":
                return DbType.SqlServer;
            case "postgresql":
                return DbType.PostgreSQL;
            case "sqlite":
                return DbType.Sqlite;
            case "oracle":
                return DbType.Oracle;
            default:
                throw new Exception("不支持的数据库类型");
        }
    }
}