namespace Athena.Infrastructure.EventTracking.Messaging.Responses;

/// <summary>
/// 
/// </summary>
public class GetTrackPagingResponse : Track
{
    /// <summary>
    /// 是否有错误
    /// </summary>
    public bool HasError => TrackStatus == TrackStatus.Fail;
}