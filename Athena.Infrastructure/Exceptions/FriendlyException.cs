using Microsoft.Extensions.Logging;

namespace Athena.Infrastructure.Exceptions;

/// <summary>
/// 友好的异常信息。
/// </summary>
public class FriendlyException : Exception
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int StatusCode { get; } = -1;

    /// <summary>
    /// 更多的消息信息
    /// </summary>
    public string? MoreMessage { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public FriendlyException()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public FriendlyException(string message) : base(message)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="moreMessage"></param>
    public FriendlyException(string message, string moreMessage) : base(message)
    {
        MoreMessage = moreMessage;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="message"></param>
    public FriendlyException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="message"></param>
    /// <param name="moreMessage"></param>
    public FriendlyException(int statusCode, string message, string moreMessage) : base(message)
    {
        StatusCode = statusCode;
        MoreMessage = moreMessage;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="throwMessage"></param>
    /// <param name="logger"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static void ThrowOf(string throwMessage, ILogger logger, string message, params object[] args)
    {
        ThrowOf(throwMessage, logger, LogLevel.Error, message, args);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="throwMessage"></param>
    /// <param name="logger"></param>
    /// <param name="logLevel"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static void ThrowOf(
        string throwMessage,
        ILogger logger,
        LogLevel logLevel,
        string message,
        params object[] args
    )
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        logger.Log(logLevel, message, args);
        throw Of(throwMessage);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static FriendlyException Of(string message)
    {
        return new FriendlyException(message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static void ThrowOf(string message)
    {
        throw new FriendlyException(message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static FriendlyException Of(int statusCode, string message)
    {
        return new FriendlyException(statusCode, message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static void ThrowOf(int statusCode, string message)
    {
        throw new FriendlyException(statusCode, message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="moreMessage"></param>
    /// <returns></returns>
    public static FriendlyException Of(string message, string moreMessage)
    {
        return new FriendlyException(message, moreMessage);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="moreMessage"></param>
    /// <returns></returns>
    public static void ThrowOf(string message, string moreMessage)
    {
        throw new FriendlyException(message, moreMessage);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="message"></param>
    /// <param name="moreMessage"></param>
    /// <returns></returns>
    public static FriendlyException Of(int statusCode, string message, string moreMessage)
    {
        return new FriendlyException(statusCode, message, moreMessage);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="message"></param>
    /// <param name="moreMessage"></param>
    /// <returns></returns>
    public static void ThrowOf(int statusCode, string message, string moreMessage)
    {
        throw new FriendlyException(statusCode, message, moreMessage);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="moreMessage"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static FriendlyException Of<T>(string message, T moreMessage)
    {
        return new FriendlyException(message, JsonSerializer.Serialize(moreMessage));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="moreMessage"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static void ThrowOf<T>(string message, T moreMessage)
    {
        throw new FriendlyException(message, JsonSerializer.Serialize(moreMessage));
    }

    /// <summary>
    /// 数据不能为空
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static FriendlyException NullArgument(string name)
    {
        return new FriendlyException($"{name}不能为空");
    }

    /// <summary>
    /// 数据不能为空
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static void ThrowNullArgument(string name)
    {
        throw new FriendlyException($"{name}不能为空");
    }

    /// <summary>
    /// 数据不能为空
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static FriendlyException NullOrEmptyArgument(string name)
    {
        return new FriendlyException($"{name}不能为空");
    }

    /// <summary>
    /// 数据不能为空
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static void ThrowNullOrEmptyArgument(string name)
    {
        throw new FriendlyException($"{name}不能为空");
    }

    /// <summary>
    /// 数据不能为空
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static FriendlyException NullOrWhiteSpaceArgument(string name)
    {
        return new FriendlyException($"{name}不能为空");
    }

    /// <summary>
    /// 数据不能为空
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static void ThrowNullOrWhiteSpaceArgument(string name)
    {
        throw new FriendlyException($"{name}不能为空");
    }

    /// <summary>
    /// 数据无效
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static FriendlyException InvalidArgument(string arg)
    {
        return new FriendlyException($"{arg}无效");
    }

    /// <summary>
    /// 数据无效
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static void ThrowInvalidArgument(string arg)
    {
        throw new FriendlyException($"{arg}无效");
    }

    /// <summary>
    /// 信息不存在
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static FriendlyException NotFound(string arg)
    {
        return new FriendlyException($"{arg}不存在");
    }

    /// <summary>
    /// 信息不存在
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static void ThrowNotFound(string arg)
    {
        throw new FriendlyException($"{arg}不存在");
    }

    /// <summary>
    /// 记录不存在
    /// </summary>
    /// <returns></returns>
    public static FriendlyException NotData()
    {
        return NotFound("记录");
    }

    /// <summary>
    /// 记录不存在
    /// </summary>
    /// <returns></returns>
    public static void ThrowNotData()
    {
        ThrowNotFound("记录");
    }
}