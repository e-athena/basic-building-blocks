using System.Net;
using System.Runtime.InteropServices;
using Athena.Infrastructure.Status;
using Microsoft.Extensions.PlatformAbstractions;

namespace Microsoft.Extensions.Configuration;

/// <summary>
/// 
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 构建应用程序状态
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public static string BuildAppStatus(this IConfiguration config)
    {
        return JsonSerializer.Serialize(config.BuildAppStatusModel());
    }

    /// <summary>
    /// 构建应用程序状态
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public static StatusModel BuildAppStatusModel(this IConfiguration config)
    {
        var model = new StatusModel
        {
            AppName = PlatformServices.Default.Application.ApplicationName,
            AppVersion = PlatformServices.Default.Application.ApplicationVersion,
            BasePath = PlatformServices.Default.Application.ApplicationBasePath
        };

        foreach (var env in config.GetChildren())
        {
            if (env.Value != null) model.Environments.Add(env.Key, env.Value);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            model.OsArchitecture = "Windows";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            model.OsArchitecture = "Linux";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            model.OsArchitecture = "OSX";
        }
        else
        {
            model.OsArchitecture = "Others";
        }
        model.OsDescription = RuntimeInformation.OSDescription;
        model.ProcessArchitecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm => "Arm",
            Architecture.Arm64 => "Arm64",
            Architecture.S390x => "S390x",
            Architecture.Wasm => "Wasm",
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            _ => "Others"
        };

        model.RuntimeFramework = PlatformServices.Default.Application.RuntimeFramework.ToString();
        model.FrameworkDescription = RuntimeInformation.FrameworkDescription;

        model.HostName = Dns.GetHostName();
        model.IpAddress = Dns.GetHostAddresses(Dns.GetHostName())
            .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .Aggregate(" ", (a, b) => $"{a} {b}");

        var process = Process.GetCurrentProcess();
        model.ProcessName = process.ProcessName;
        model.MemoryUsage = $"{process.WorkingSet64 / 1024 / 1024}Mb";
        model.VirtualMemory = $"{process.VirtualMemorySize64 / 1024 / 1024 / 1024}Gb";
        model.StartTime = process.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
        model.Threads = process.Threads.Count;
        model.TotalProcessorTime = $"{process.TotalProcessorTime}";
        model.UserProcessorTime = $"{process.UserProcessorTime}";

        return model;
    }
}