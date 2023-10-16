using System.ComponentModel;

namespace Athena.Infrastructure.Swagger;

/// <summary>
/// 
/// </summary>
public enum ApiVersions
{
    /// <summary>
    /// 第一版
    /// </summary>
    [Description("V1")] V1,

    /// <summary>
    /// 第二版
    /// </summary>
    [Description("V2")] V2,

    /// <summary>
    /// 第三版
    /// </summary>
    [Description("V3")] V3,

    /// <summary>
    /// 第四版
    /// </summary>
    [Description("V4")] V4,

    /// <summary>
    /// 第五版
    /// </summary>
    [Description("V5")] V5
}