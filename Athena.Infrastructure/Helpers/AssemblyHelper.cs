using System.Collections.Concurrent;
using Microsoft.Extensions.PlatformAbstractions;

namespace Athena.Infrastructure.Helpers;

/// <summary>
/// 程序集帮助类
/// </summary>
public static class AssemblyHelper
{
    /// <summary>
    /// 读取当前程序集列表
    /// </summary>
    /// <param name="assemblyKeyword"></param>
    /// <returns></returns>
    public static Assembly[] GetCurrentDomainBusinessAssemblies(string? assemblyKeyword = null)
    {
        // 跳过以System.、Microsoft.、FreeSql.、DotNetCore.CAP.、MediatR.、Serilog.、NewLife.、Athena.、Rougamo.、Swashbuckle.、SQLitePCLRaw.、Newtonsoft.、MessagePack.开头的程序集
        var assemblies = GetCurrentDomainAssemblies(assemblyKeyword)
            .Where(p => !p.FullName!.StartsWith("System") && !p.FullName.StartsWith("Microsoft") &&
                        !p.FullName.StartsWith("FreeSql.") && !p.FullName.StartsWith("DotNetCore.CAP") &&
                        !p.FullName.StartsWith("MediatR") && !p.FullName.StartsWith("Serilog") &&
                        !p.FullName.StartsWith("NewLife") && !p.FullName.StartsWith("Athena.") &&
                        !p.FullName.StartsWith("Rougamo") && !p.FullName.StartsWith("Swashbuckle") &&
                        !p.FullName.StartsWith("SQLitePCLRaw") && !p.FullName.StartsWith("Newtonsoft") &&
                        !p.FullName.StartsWith("MessagePack") && !p.FullName.StartsWith("Anonymously Hosted") &&
                        !p.FullName.StartsWith("netstandard") && !p.FullName.StartsWith("StackExchange") &&
                        !p.FullName.StartsWith("CSRedisCore") && !p.FullName.StartsWith("Caching.CSRedis") &&
                        !p.FullName.StartsWith("Pipelines") && !p.FullName.StartsWith("FreeScheduler") &&
                        !p.FullName.StartsWith("IdleBus") && !p.FullName.StartsWith("FreeSql,") &&
                        !p.FullName.StartsWith("SqlSugar,") && !p.FullName.StartsWith("MySqlConnector,") &&
                        !p.FullName.StartsWith("Npgsql,") && !p.FullName.StartsWith("K4os") &&
                        !p.FullName.StartsWith("UAParser") && !p.FullName.StartsWith("Ubiety.Dns.Core") &&
                        !p.FullName.StartsWith("Google.Protobuf") && !p.FullName.StartsWith("WorkQueue") &&
                        !p.FullName.StartsWith("Consul") && !p.FullName.StartsWith("ZstdNet") &&
                        !p.FullName.StartsWith("K4os.Hash.xxHash") && !p.FullName.StartsWith("Scrutor") &&
                        !p.FullName.StartsWith("FluentValidation") && !p.FullName.StartsWith("K4os.Compression.LZ4") &&
                        !p.FullName.StartsWith("BouncyCastle.Crypto") && !p.FullName.StartsWith("MySql.Data")
            )
            .ToArray();

        return assemblies;
    }

    /// <summary>
    /// 读取当前程序集列表
    /// </summary>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    public static Assembly[] GetCurrentDomainBusinessAssemblies(params string[] assemblyKeywords)
    {
        // 跳过以System.、Microsoft.、FreeSql.、DotNetCore.CAP.、MediatR.、Serilog.、NewLife.、Athena.、Rougamo.、Swashbuckle.、SQLitePCLRaw.、Newtonsoft.、MessagePack.开头的程序集
        var assemblies = GetCurrentDomainAssemblies(assemblyKeywords)
            .Where(p => !p.FullName!.StartsWith("System") && !p.FullName.StartsWith("Microsoft") &&
                        !p.FullName.StartsWith("FreeSql.") && !p.FullName.StartsWith("DotNetCore.CAP") &&
                        !p.FullName.StartsWith("MediatR") && !p.FullName.StartsWith("Serilog") &&
                        !p.FullName.StartsWith("NewLife") && !p.FullName.StartsWith("Athena") &&
                        !p.FullName.StartsWith("Rougamo") && !p.FullName.StartsWith("Swashbuckle") &&
                        !p.FullName.StartsWith("SQLitePCLRaw") && !p.FullName.StartsWith("Newtonsoft") &&
                        !p.FullName.StartsWith("MessagePack") && !p.FullName.StartsWith("Anonymously Hosted") &&
                        !p.FullName.StartsWith("netstandard") && !p.FullName.StartsWith("StackExchange") &&
                        !p.FullName.StartsWith("CSRedisCore") && !p.FullName.StartsWith("Caching.CSRedis") &&
                        !p.FullName.StartsWith("Pipelines") && !p.FullName.StartsWith("FreeScheduler") &&
                        !p.FullName.StartsWith("IdleBus") && !p.FullName.StartsWith("FreeSql,") &&
                        !p.FullName.StartsWith("SqlSugar,") && !p.FullName.StartsWith("MySqlConnector,") &&
                        !p.FullName.StartsWith("Npgsql,") && !p.FullName.StartsWith("K4os") &&
                        !p.FullName.StartsWith("UAParser") && !p.FullName.StartsWith("Ubiety.Dns.Core") &&
                        !p.FullName.StartsWith("Google.Protobuf") && !p.FullName.StartsWith("WorkQueue") &&
                        !p.FullName.StartsWith("Consul") && !p.FullName.StartsWith("ZstdNet") &&
                        !p.FullName.StartsWith("K4os.Hash.xxHash") && !p.FullName.StartsWith("Scrutor") &&
                        !p.FullName.StartsWith("FluentValidation") && !p.FullName.StartsWith("K4os.Compression.LZ4") &&
                        !p.FullName.StartsWith("BouncyCastle.Crypto") && !p.FullName.StartsWith("MySql.Data")
            )
            .ToArray();

        return assemblies;
    }

    /// <summary>
    /// 读取当前程序集列表
    /// </summary>
    /// <param name="assemblyKeyword"></param>
    /// <returns></returns>
    public static Assembly[] GetCurrentDomainAssemblies(string? assemblyKeyword = null)
    {
        // 加载PlatformServices.Default.Application.ApplicationBasePath下所有的程序集
        var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
        var assemblies = LoadAssembliesFromFolder(applicationBasePath);
        // return AppDomain.CurrentDomain.GetAssemblies()
        return assemblies
            .HasWhere(!string.IsNullOrEmpty(assemblyKeyword), p => p.FullName!.Contains(assemblyKeyword!))
            .ToArray();
    }

    /// <summary>
    /// 读取当前程序集列表
    /// </summary>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    public static Assembly[] GetCurrentDomainAssemblies(params string[] assemblyKeywords)
    {
        // 加载PlatformServices.Default.Application.ApplicationBasePath下所有的程序集
        var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
        var assemblies = LoadAssembliesFromFolder(applicationBasePath);
        // return AppDomain.CurrentDomain.GetAssemblies()
        return assemblies
            .HasWhere(assemblyKeywords is {Length: > 0},
                p => assemblyKeywords.Any(assemblyKeyword => p.FullName!.Contains(assemblyKeyword)))
            .ToArray();
    }

    // 缓存指定目录下的所有程序集，避免每次都重新加载
    private static readonly ConcurrentDictionary<string, Assembly[]> AssemblyCache = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public static Assembly[] LoadAssembliesFromFolder(string folderPath)
    {
        // 先读取缓存
        if (AssemblyCache.TryGetValue(folderPath, out var cacheAssemblies))
        {
            return cacheAssemblies;
        }

        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {folderPath}");
        }

        var assemblies = new List<Assembly>();
        var files = Directory.GetFiles(folderPath, "*.dll");

        foreach (var file in files)
        {
            try
            {
                var assembly = Assembly.LoadFrom(file);
                assemblies.Add(assembly);
            }
            catch (Exception)
            {
                // Handle any exceptions that occur while loading the assembly
            }
        }

        // 缓存
        AssemblyCache.TryAdd(folderPath, assemblies.ToArray());

        return assemblies.ToArray();
    }
}