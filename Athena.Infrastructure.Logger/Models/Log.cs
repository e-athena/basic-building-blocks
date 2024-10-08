namespace Athena.Infrastructure.Logger.Models;

/// <summary>
/// 日志模型
/// </summary>
[Table("logs")]
[Index("created_on")]
[Index("user_id")]
public class Log
{
    /// <summary>
    /// ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// 服务名
    /// </summary>
    [MaxLength(32)]
    [Column("service_name")]
    public string ServiceName { get; set; } = "CommonService";

    /// <summary>
    /// 别名
    /// </summary>
    [MaxLength(32)]
    [Column("alias_name")]
    public string? AliasName { get; set; }

    /// <summary>
    /// 追踪ID
    /// </summary>
    [MaxLength(36)]
    [Column("trace_id")]
    public string? TraceId { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    [MaxLength(32)]
    [Column("ip_address")]
    public string? IpAddress { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    [MaxLength(256)]
    [Column("user_agent")]
    public string? UserAgent { get; set; }

    /// <summary>
    /// 日志等级
    /// </summary>
    [Column("log_level")]
    public LogLevel LogLevel { get; set; }

    /// <summary>
    /// 路由
    /// </summary>
    [Column("route")]
    public string? Route { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    [Column("http_method")]
    public string? HttpMethod { get; set; }

    /// <summary>
    /// 请求主体
    /// </summary>
    [MaxLength(-2)]
    [Column("request_body")]
    public string? RequestBody { get; set; }

    /// <summary>
    /// 响应主体
    /// </summary>
    [MaxLength(-2)]
    [Column("response_body")]
    public string? ResponseBody { get; set; }

    /// <summary>
    /// 原始数据
    /// </summary>
    [MaxLength(-2)]
    [Column("raw_data")]
    public string? RawData { get; set; }

    /// <summary>
    /// 状态码
    /// </summary>
    [Column("status_code")]
    public int StatusCode { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    [MaxLength(36)]
    [Column("user_id")]
    public string? UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(32)]
    [Column("user_name")]
    public string? UserName { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [Column("start_time")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [Column("end_time")]
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 处理耗时/ms
    /// </summary>
    [Column("elapsed_milliseconds")]
    public long ElapsedMilliseconds { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column("created_on")]
    public DateTime CreatedOn { get; set; } = DateTime.Now;

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
    [JsonIgnore]
    public string? Browser => UserAgentInfo?.UA.ToString();

    /// <summary>
    /// 操作系统
    /// </summary>
    [JsonIgnore]
    public string? Os => UserAgentInfo?.OS.ToString();

    /// <summary>
    /// 设备
    /// </summary>
    [JsonIgnore]
    public string? Device => UserAgentInfo?.Device.ToString();

    #endregion
}