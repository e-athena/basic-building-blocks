namespace Athena.Infrastructure.CronJobs;

/// <summary>
/// 循环作业默认接口
/// </summary>
public interface ICronJobWorker
{
    /// <summary>
    /// 作业ID
    /// </summary>
    string JobId { get; }

    /// <summary>
    /// Cron表达式
    /// </summary>
    Func<string>? CronExpression { get; }

    /// <summary>
    /// Do somethings
    /// </summary>
    /// <returns></returns>
    Task DoWorkAsync();

    /// <summary>
    /// 队列
    /// <remarks>默认值：default</remarks>
    /// </summary>
    string? Queue { get; }
}