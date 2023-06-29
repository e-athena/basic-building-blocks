using System.Reflection;
using System.Text.RegularExpressions;

namespace Athena.Infrastructure.FreeSql.Helpers;

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
    public static DataType GetDataType(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return DataType.Custom;
        }
        // 使用正则表达式判断数据库类型

        if (Regex.IsMatch(connectionString, @"Data Source=\S+\.db;|\b(SQLite)\b"))
        {
            return DataType.Sqlite;
        }

        if (Regex.IsMatch(connectionString, @"\b(MySQL|MariaDB)\b", RegexOptions.IgnoreCase))
        {
            return DataType.MySql;
        }

        if (Regex.IsMatch(connectionString, @"\b(Data Source|Server)\b", RegexOptions.IgnoreCase))
        {
            return DataType.SqlServer;
        }

        if (Regex.IsMatch(connectionString,
                @"Server=\S+;Port=\d+;Database=\S+;User Id=\S+;Password=\S+;|\b(PostgreSQL)\b"))
        {
            return DataType.PostgreSQL;
        }

        return DataType.Custom;
    }

    /// <summary>
    /// 读取数据库类型
    /// </summary>
    /// <param name="connectionString">连接字符串</param>
    /// <returns></returns>
    public static (DataType dataType, string connectionString) GetDataTypeAndConnectionString(string connectionString)
    {
        // 使用逗号分隔连接字符串，第一个为数据库类型，第二个为数据库连接字符串
        var array = connectionString.Split(',');
        var res = array.Length == 1
            ? (GetDataType(connectionString), connectionString)
            : (GetDataTypeByName(array[0]), array[1]);
        CheckDependency(res.Item1);
        return res;
    }

    /// <summary>
    /// 根据名称获取数据库类型
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static DataType GetDataTypeByName(string name)
    {
        switch (name.ToLower())
        {
            case "mysql":
            case "mariadb":
                return DataType.MySql;
            case "sqlserver":
                return DataType.SqlServer;
            case "postgresql":
                return DataType.PostgreSQL;
            case "sqlite":
                return DataType.Sqlite;
            case "oracle":
                return DataType.Oracle;
            default:
                return DataType.Custom;
        }
    }

    /// <summary>
    /// 检查依赖
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static void CheckDependency(DataType dataType)
    {
        switch (dataType)
        {
            // 根据数据库类型检查是否存在程序集依赖
            case DataType.MySql:
            {
                var assembly1 = LoadAssembly("FreeSql.Provider.MySqlConnector");
                var assembly2 = LoadAssembly("FreeSql.Provider.MySql");
                if (assembly1 == null && assembly2 == null)
                {
                    throw new Exception(
                        "未找到FreeSql.Provider.MySqlConnector或FreeSql.Provider.MySql程序集，请检查是否已安装FreeSql.Provider.MySqlConnector或FreeSql.Provider.MySql包");
                }

                break;
            }
            case DataType.Sqlite:
            {
                var assembly1 = LoadAssembly("FreeSql.Provider.SqliteCore");
                var assembly2 = LoadAssembly("FreeSql.Provider.Sqlite");
                if (assembly1 == null && assembly2 == null)
                {
                    throw new Exception(
                        "未找到FreeSql.Provider.SqliteCore或FreeSql.Provider.Sqlite程序集，请检查是否已安装FreeSql.Provider.SqliteCore或FreeSql.Provider.Sqlite包");
                }

                break;
            }
            case DataType.SqlServer:
            {
                var assembly = LoadAssembly("FreeSql.Provider.SqlServer");
                if (assembly == null)
                {
                    throw new Exception("未找到FreeSql.Provider.SqlServer程序集，请检查是否已安装FreeSql.Provider.SqlServer包");
                }

                break;
            }
            case DataType.PostgreSQL:
            {
                var assembly = LoadAssembly("FreeSql.Provider.PostgreSQL");
                if (assembly == null)
                {
                    throw new Exception("未找到FreeSql.Provider.PostgreSQL程序集，请检查是否已安装FreeSql.Provider.PostgreSQL包");
                }

                break;
            }
            default:
                throw new Exception("不支持的数据库类型");
        }
    }

    /// <summary>
    /// 加载程序集
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    private static Assembly? LoadAssembly(string assemblyName)
    {
        try
        {
            return Assembly.Load(assemblyName);
        }
        catch
        {
            return null;
        }
    }
}