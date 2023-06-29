using Athena.Infrastructure.Messaging.Responses;

namespace Athena.Infrastructure.QiNiuCloud.Helpers;

/// <summary>
/// 结果集帮助类
/// </summary>
public static class ResultHelper
{
    /// <summary>
    /// [异步]通用的执行结果方法
    /// </summary>
    /// <param name="func"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static async Task<ApiResult<TResult>> CommonResultAsync<TResult>(Func<Task<TResult>> func)
    {
        var rsp = new ApiResult<TResult>();
        try
        {
            rsp.StatusCode = 200; // 状态码
            rsp.Message = "succeed"; // 信息
            rsp.Success = true; // 是否成功
            rsp.Data = await func(); // 数据集
        }
        catch (Exception ex)
        {
            rsp.Success = false;
            rsp.StatusCode = 500;
            rsp.Message = ex.Message;
            rsp.InnerMessage = ex.InnerException?.Message;
            rsp.StackTrace = ex.StackTrace;
        }

        return rsp;
    }

    /// <summary>
    /// 通用的执行结果方法
    /// </summary>
    /// <param name="func"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static ApiResult<TResult> CommonResult<TResult>(Func<TResult> func)
    {
        var rsp = new ApiResult<TResult>();
        try
        {
            rsp.StatusCode = 200; // 状态码
            rsp.Message = "succeed"; // 信息
            rsp.Success = true; // 是否成功
            rsp.Data = func(); // 数据集
        }
        catch (Exception ex)
        {
            rsp.Success = false;
            rsp.StatusCode = 500;
            rsp.Message = ex.Message;
            rsp.InnerMessage = ex.InnerException?.Message;
            rsp.StackTrace = ex.StackTrace;
        }

        return rsp;
    }

    /// <summary>
    /// [异步]通用的执行结果方法
    /// </summary>
    /// <param name="func"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static async Task<ApiResult<TResult>> CommonResultAsync<TResult>(Func<Task<ApiResult<TResult>>> func)
    {
        var rsp = new ApiResult<TResult>();
        try
        {
            var result = await func();
            rsp.StatusCode = result.StatusCode; // 状态码
            rsp.Message = result.Message; // 信息
            rsp.Success = result.Success; // 是否成功
            rsp.Data = result.Data; // 数据集
        }
        catch (Exception ex)
        {
            rsp.Success = false;
            rsp.StatusCode = 500;
            rsp.Message = ex.Message;
            rsp.InnerMessage = ex.InnerException?.Message;
            rsp.StackTrace = ex.StackTrace;
        }

        return rsp;
    }
}