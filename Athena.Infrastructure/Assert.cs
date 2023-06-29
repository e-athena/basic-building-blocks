using Athena.Infrastructure.Exceptions;

namespace Athena.Infrastructure;

/// <summary>
/// 
/// </summary>
public static class Assert
{
    /// <summary>
    /// 如果为Null则抛出异常
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    /// <exception cref="FriendlyException"></exception>
    public static void IsNotNull(string name, object obj)
    {
        if (obj == null)
        {
            throw FriendlyException.NullArgument(name);
        }
    }

    /// <summary>
    /// 如果为Null则抛出异常
    /// </summary>
    /// <param name="name"></param>
    /// <param name="input"></param>
    /// <exception cref="FriendlyException"></exception>
    public static void IsNotNullOrEmpty(string name, string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw FriendlyException.NullOrEmptyArgument(name);
        }
    }

    /// <summary>
    /// 如果为Null或空格则抛出异常
    /// </summary>
    /// <param name="name"></param>
    /// <param name="input"></param>
    /// <exception cref="FriendlyException"></exception>
    public static void IsNotNullOrWhiteSpace(string name, string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw FriendlyException.NullOrWhiteSpaceArgument(name);
        }
    }

    /// <summary>
    /// 是否为真
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="errorMessage"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void IsTrue(bool condition, string errorMessage)
    {
        if (!condition)
        {
            throw FriendlyException.Of(errorMessage);
        }
    }

    /// <summary>
    /// 如果两个对象不相等则抛出异常
    /// </summary>
    /// <param name="id1"></param>
    /// <param name="id2"></param>
    /// <param name="errorMessageFormat"></param>
    /// <exception cref="FriendlyException"></exception>
    public static void AreEqual(string id1, string id2, string errorMessageFormat)
    {
        if (id1 != id2)
        {
            throw FriendlyException.Of(string.Format(errorMessageFormat, id1, id2));
        }
    }

    /// <summary>
    /// 如果两个对象相等则抛出异常
    /// </summary>
    /// <param name="id1"></param>
    /// <param name="id2"></param>
    /// <param name="errorMessageFormat"></param>
    /// <exception cref="FriendlyException"></exception>
    public static void Equal(string id1, string id2, string errorMessageFormat)
    {
        if (id1 == id2)
        {
            throw FriendlyException.Of(string.Format(errorMessageFormat, id1, id2));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dateBegin"></param>
    /// <param name="dateEnd"></param>
    /// <returns></returns>
    public static TimeSpan ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
    {
        var tsBegin = new TimeSpan(dateBegin.Ticks);
        var tsEnd = new TimeSpan(dateEnd.Ticks);
        return tsBegin.Subtract(tsEnd).Duration();
    }
}