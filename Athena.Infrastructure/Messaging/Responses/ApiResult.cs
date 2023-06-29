namespace Athena.Infrastructure.Messaging.Responses;

/// <summary>
/// APIResult 接口统一返回类
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class ApiResult<T> : ApiResultBase
{
    /// <summary>
    /// 数据集
    /// </summary>
    public T? Data { get; set; }
}