namespace Athena.Infrastructure.SubApplication;

/// <summary>
/// 请求基类接口
/// </summary>
public interface IRequestBase
{
    /// <summary>
    /// 方法名
    /// </summary>
    string MethodName { get; }
}