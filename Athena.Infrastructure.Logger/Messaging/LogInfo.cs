namespace Athena.Infrastructure.Logger.Messaging;

/// <summary>
/// 
/// </summary>
public class LogInfo
{
    /// <summary>
    /// ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 服务名
    /// </summary>
    public string ServiceName { get; set; } = null!;

    /// <summary>
    /// 别名
    /// </summary>
    public string? AliasName { get; set; }

    /// <summary>
    /// 追踪ID
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 日志等级
    /// </summary>
    public LogLevel LogLevel { get; set; }

    /// <summary>
    /// 路由
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// 请求主体
    /// </summary>
    [MaxLength(-2)]
    public string? RequestBody { get; set; }

    /// <summary>
    /// 响应主体
    /// </summary>
    [MaxLength(-2)]
    public string? ResponseBody { get; set; }

    /// <summary>
    /// 原始数据
    /// </summary>
    [MaxLength(-2)]
    public string? RawData { get; set; }

    /// <summary>
    /// 状态码
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 处理耗时/ms
    /// </summary>
    public long ElapsedMilliseconds { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedOn { get; set; }

    #region UserAgent相关

    /// <summary>
    /// 
    /// </summary>
    private ClientInfo? _userAgentInfo;

    /// <summary>
    /// 
    /// </summary>
    private ClientInfo? UserAgentInfo
    {
        get
        {
            if (_userAgentInfo != null)
            {
                return _userAgentInfo;
            }

            if (string.IsNullOrEmpty(UserAgent))
            {
                return null;
            }

            _userAgentInfo = Parser.GetDefault().Parse(UserAgent);
            return _userAgentInfo;
        }
    }

    /// <summary>
    /// 浏览器
    /// </summary>
    public string? Browser => UserAgentInfo?.UA.ToString();

    /// <summary>
    /// 操作系统
    /// </summary>
    public string? Os => UserAgentInfo?.OS.ToString();

    /// <summary>
    /// 设备
    /// </summary>
    public string? Device => UserAgentInfo?.Device.ToString();

    #endregion
}