namespace Athena.Infrastructure.SubApplication.Services.Impls;

/// <summary>
/// 默认服务基类
/// </summary>
public class DefaultServiceBase
{
    private readonly string? _basicAuthUserName;
    private readonly string? _basicAuthPassword;
    private readonly ISecurityContextAccessor _accessor;
    private string _apiUrl = "http://localhost:5078";
    private ServiceCallConfig? _callConfig;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="accessor"></param>
    public DefaultServiceBase(ISecurityContextAccessor accessor)
    {
        _accessor = accessor;
        if (AthenaProvider.Provider?.GetService(typeof(IConfiguration)) is not IConfiguration config)
        {
            return;
        }

        _basicAuthUserName = config.GetSection("BasicAuthConfig").GetValue<string>("UserName");
        _basicAuthPassword = config.GetSection("BasicAuthConfig").GetValue<string>("Password");

        // 读取配置
        if (AthenaProvider.Provider.GetService(typeof(IOptionsMonitor<ServiceCallConfig>)) is
            IOptionsMonitor<ServiceCallConfig>
            {
                CurrentValue.CallType: ServiceCallType.Http
            } options)
        {
            _apiUrl = options.CurrentValue.HttpApiUrl!;
            _callConfig = options.CurrentValue;

            options.OnChange(newOptions =>
            {
                _apiUrl = newOptions.HttpApiUrl!;
                _callConfig = newOptions;
            });
            return;
        }

        var url = config.GetSection("MainApplicationApiUrl").Value;
        if (url != null)
        {
            _apiUrl = url;
        }
    }

    /// <summary>
    /// 获取请求对象
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    protected IFlurlRequest GetRequest(string url)
    {
        return $"{_apiUrl}{url}"
            .WithTimeout(_callConfig?.Timeout ?? 30)
            .WithHeader("AppId", _accessor.AppId)
            .WithHeader("TenantId", _accessor.TenantId)
            .WithOAuthBearerToken(_accessor.JwtTokenNotBearer)
            .OnError(act =>
            {
                var res = act.Response;
                if (res != null)
                {
                    throw new FriendlyException(res.StatusCode, res.ResponseMessage.ReasonPhrase ?? "未知错误", url);
                }
            });
    }

    /// <summary>
    /// 获取请求对象
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    /// <exception cref="FriendlyException"></exception>
    protected IFlurlRequest GetRequestWithBasicAuth(string url)
    {
        if (_basicAuthUserName == null || _basicAuthPassword == null)
        {
            throw new FriendlyException("未配置BasicAuth");
        }

        return $"{_apiUrl}{url}"
            .WithTimeout(_callConfig?.Timeout ?? 30)
            .WithBasicAuth(_basicAuthUserName, _basicAuthPassword)
            .OnError(act =>
            {
                var res = act.Response;
                if (res != null)
                {
                    throw new FriendlyException(res.StatusCode, res.ResponseMessage.ReasonPhrase ?? "未知错误", url);
                }
            });
    }
}