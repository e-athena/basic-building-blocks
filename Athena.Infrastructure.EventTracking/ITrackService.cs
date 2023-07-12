namespace Athena.Infrastructure.EventTracking;

/// <summary>
/// 追踪服务接口
/// </summary>
public interface ITrackService
{
    /// <summary>
    /// 写入追踪数据
    /// </summary>
    /// <param name="track"></param>
    /// <returns></returns>
    void Write(Track track);

    /// <summary>
    /// 写入追踪数据
    /// </summary>
    /// <param name="track"></param>
    /// <returns></returns>
    Task WriteAsync(Track track);
}