using System.Diagnostics;

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