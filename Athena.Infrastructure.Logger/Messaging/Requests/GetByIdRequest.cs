namespace Athena.Infrastructure.Logger.Messaging.Requests;

/// <summary>
/// 读取日志详情
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class GetByIdRequest : RequestBase
{
    /// <summary>
    /// ID
    /// </summary>
    public long Id { get; set; }
}