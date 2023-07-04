using Athena.Infrastructure.Exceptions;

namespace Athena.Infrastructure.EventTracking.Messaging.Requests;

/// <summary>
/// 保存配置信息请求类
/// </summary>
public class SaveTrackConfigRequest
{
    /// <summary>
    /// 配置
    /// </summary>
    public List<TrackConfig> Configs { get; set; } = null!;

    /// <summary>
    /// 读取根节点配置
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FriendlyException"></exception>
    public TrackConfig GetRootConfig()
    {
        Configs.ForEach(config => { config.Check(); });
        var rootConfig = Configs.FirstOrDefault(config => config.ParentId == null);
        if (rootConfig == null)
        {
            throw FriendlyException.Of("根节点不能为空");
        }

        return rootConfig;
    }
}