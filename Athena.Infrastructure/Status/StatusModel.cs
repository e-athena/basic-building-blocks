namespace Athena.Infrastructure.Status
{
    /// <summary>
    /// 状态模型
    /// </summary>
    public class StatusModel
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        public string? AppName { get; set; }

        /// <summary>
        /// 应用版本
        /// </summary>
        public string? AppVersion { get; set; }

        /// <summary>
        /// 操作系统架构
        /// </summary>
        public string? OsArchitecture { get; set; }

        /// <summary>
        /// 操作系统说明
        /// </summary>
        public string? OsDescription { get; set; }

        /// <summary>
        /// 流程架构
        /// </summary>
        public string? ProcessArchitecture { get; set; }

        /// <summary>
        /// 基本路径
        /// </summary>
        public string? BasePath { get; set; }

        /// <summary>
        /// 运行时框架
        /// </summary>
        public string? RuntimeFramework { get; set; }

        /// <summary>
        /// 框架说明
        /// </summary>
        public string? FrameworkDescription { get; set; }

        /// <summary>
        /// 主机名
        /// </summary>
        public string? HostName { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string? IpAddress { get; set; }


        /// <summary>
        /// 进程名称
        /// </summary>
        public string? ProcessName { get; set; }

        /// <summary>
        /// 虚拟内存
        /// </summary>
        public string? VirtualMemory { get; set; }

        /// <summary>
        /// 内存使用情况
        /// </summary>
        public string? MemoryUsage { get; set; }

        /// <summary>
        /// 启动时间
        /// </summary>
        public string? StartTime { get; set; }

        /// <summary>
        /// 线程数
        /// </summary>
        public int Threads { get; set; }

        /// <summary>
        /// 处理器总时间
        /// </summary>
        public string? TotalProcessorTime { get; set; }

        /// <summary>
        /// 用户处理器时间
        /// </summary>
        public string? UserProcessorTime { get; set; }

        /// <summary>
        /// 环境变量
        /// </summary>
        public IDictionary<string, object> Environments { get; set; } = new Dictionary<string, object>();
    }
}