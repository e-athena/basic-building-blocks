using Athena.Infrastructure.Messaging.Responses;
using Athena.Infrastructure.ViewModels;

namespace Athena.Infrastructure.Logger;

/// <summary>
/// 日志存储服务接口
/// </summary>
public interface ILoggerStorageService
{
    /// <summary>
    /// 写日志
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    Task WriteAsync(Log log);

    /// <summary>
    /// 读取分页列表
    /// </summary>
    /// <param name="request">ID</param>
    /// <returns></returns>
    Task<Paging<GetLogPagingResponse>> GetPagingAsync(GetLogPagingRequest request);

    /// <summary>
    /// 读取日志详情
    /// </summary>
    /// <param name="request">ID</param>
    /// <returns></returns>
    Task<GetByIdResponse> GetByIdAsync(GetByIdRequest request);

    /// <summary>
    /// 读取日志详情
    /// </summary>
    /// <param name="request">ID</param>
    /// <returns></returns>
    Task<GetByTraceIdResponse> GetByTraceIdAsync(GetByTraceIdRequest request);

    /// <summary>
    /// 读取调用次数
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<long> GetCallCountAsync(GetCallCountRequest request);

    /// <summary>
    /// 读取服务列表
    /// </summary>
    /// <returns></returns>
    Task<List<SelectViewModel>> GetServiceSelectListAsync();
}