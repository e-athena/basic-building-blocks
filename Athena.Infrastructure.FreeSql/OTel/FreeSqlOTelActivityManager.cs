using System.Diagnostics.Metrics;

namespace Athena.Infrastructure.FreeSql.OTel;

/// <summary>
/// 
/// </summary>
public static class FreeSqlOTelActivityManager
{
    /// <summary>
    /// ActivitySourceName
    /// </summary>
    public const string ActivitySourceName = "OpenTelemetry.Instrumentation.FreeSql";

    /// <summary>
    /// 实例
    /// </summary>
    public static ActivitySource Instance { get; } = new(ActivitySourceName, "1.0.0.0");
}

/// <summary>
/// It is recommended to use a custom type to hold references for
/// ActivitySource and Instruments. This avoids possible type collisions
/// with other components in the DI container.
/// </summary>
public class FreeSqlInstrumentation : IDisposable
{
    // private const string ActivitySourceName = "BasicPlatform.WebAPI";
    // private const string MeterName = "BasicPlatform.WebAPI";
    private readonly Meter _meter;

    /// <summary>
    ///
    /// </summary>
    public FreeSqlInstrumentation()
    {
        var version = typeof(FreeSqlInstrumentation).Assembly.GetName().Version?.ToString();
        ActivitySource = new ActivitySource(FreeSqlOTelActivityManager.ActivitySourceName, version);
        _meter = new Meter(FreeSqlOTelActivityManager.ActivitySourceName, version);

        QueryListCounter = _meter.CreateCounter<long>("freesql.query.list.counter",
            description: "The count of FreeSql query list");

        QueryDurationSeconds = _meter.CreateHistogram<double>("freesql.query.duration.seconds",
            description: "The duration of FreeSql query in seconds");
    }

    /// <summary>
    ///
    /// </summary>
    public ActivitySource ActivitySource { get; }

    /// <summary>
    /// 添加查询耗时计数器
    /// </summary>
    public Counter<long> QueryListCounter { get; }

    /// <summary>
    /// 添加统计耗时的直方图
    /// </summary>
    public Histogram<double> QueryDurationSeconds { get; }

    /// <summary>
    ///
    /// </summary>
    public void Dispose()
    {
        ActivitySource.Dispose();
        _meter.Dispose();
    }
}