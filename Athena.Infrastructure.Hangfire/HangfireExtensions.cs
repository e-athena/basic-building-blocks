using Athena.Infrastructure.CronJobs;
using Athena.Infrastructure.Hangfire;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Hangfire扩展类
/// </summary>
public static class HangfireExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString"></param>
    /// <param name="redisStorageOptions"></param>
    /// <param name="optionsAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomHangfireWithRedis(
        this IServiceCollection services,
        string connectionString,
        RedisStorageOptions? redisStorageOptions = null,
        Action<BackgroundJobServerOptions>? optionsAction = null)
    {
        // Add Hangfire services.
        services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseRedisStorage(connectionString, redisStorageOptions)
        );

        // Add the processing server as IHostedService
        services.AddHangfireServer(optionsAction);
        return services;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="redisStorageOptions"></param>
    /// <param name="optionsAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomHangfireWithRedis(
        this IServiceCollection services,
        IConfiguration configuration,
        RedisStorageOptions? redisStorageOptions = null,
        Action<BackgroundJobServerOptions>? optionsAction = null)
    {
        var config = GetConfig(configuration);
        redisStorageOptions ??= new RedisStorageOptions
        {
            SucceededListSize = 10000,
            DeletedListSize = 1000,
            Prefix = "hangfire:"
        };
        // Add Hangfire services.
        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseRedisStorage(nameOrConnectionString: config.ConnectionString, redisStorageOptions)
        );

        // Add the processing server as IHostedService
        services.AddHangfireServer(options =>
        {
            options.Queues = config.Queues ?? new[] {"default"};
            optionsAction?.Invoke(options);
        });
        return services;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <param name="prefixPath"></param>
    /// <param name="pathMatch"></param>
    /// <param name="account"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseCustomHangfireDashboard(this IApplicationBuilder app,
        string prefixPath = "",
        string pathMatch = "/hangfire",
        string account = "admin",
        string password = "@@123456"
    )
    {
        pathMatch = string.IsNullOrWhiteSpace(pathMatch) ? "/hangfire" : pathMatch;
        account = string.IsNullOrWhiteSpace(account) ? "admin" : account;
        password = string.IsNullOrWhiteSpace(password) ? "admin" : password;
        app.UseHangfireDashboard(pathMatch, new DashboardOptions
        {
            PrefixPath = prefixPath,
            Authorization = new IDashboardAuthorizationFilter[]
            {
                new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                {
                    // Require secure connection for dashboard
                    RequireSsl = false,
                    SslRedirect = false,
                    // Case sensitive login checking
                    LoginCaseSensitive = true,
                    // Users
                    Users = new[]
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login = account,
                            // Password as plain text, SHA1 will be used
                            PasswordClear = password
                        },
                    }
                }),
            },
        });
        return app;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <param name="configuration"></param>
    /// <param name="prefixPath"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseCustomHangfireDashboard(
        this IApplicationBuilder app,
        IConfiguration configuration,
        string prefixPath = ""
    )
    {
        var pathMatch = "/hangfire";
        var account = "admin";
        var password = "admin";
        var config = GetConfig(configuration);
        if (config.Dashboard == null)
        {
            return app.UseCustomHangfireDashboard(prefixPath, pathMatch, account, password);
        }

        pathMatch = config.Dashboard.PathMatch;
        account = config.Dashboard.Account;
        password = config.Dashboard.Password;

        return app.UseCustomHangfireDashboard(prefixPath, pathMatch, account, password);
    }

    /// <summary>
    /// 系统轮询任务
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseCustomHangfireRecurringJob(this IApplicationBuilder app)
    {
        var cronJobWorkers = app.ApplicationServices.GetService<IEnumerable<ICronJobWorker>>();

        // 循环作业
        if (cronJobWorkers == null)
        {
            return app;
        }

        var jobWorkers = cronJobWorkers.ToList();
        if (!jobWorkers.Any())
        {
            return app;
        }

        var jobWorker = jobWorkers
            .FirstOrDefault(p => p.CronExpression == null);
        // 有服务未配置表达式
        if (jobWorker != null)
        {
            throw new Exception($"{jobWorker.GetType().FullName}未配置表达式[CronExpression]");
        }

        foreach (var worker in jobWorkers)
        {
            var jobId = worker.JobId;
            if (string.IsNullOrEmpty(jobId))
            {
                jobId = worker.GetType().FullName;
            }

            // var queue = worker.Queue ?? "default";
            RecurringJob.AddOrUpdate(
                jobId,
                // queue,
                () => worker.DoWorkAsync(),
                worker.CronExpression,
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Local
                }
            );
        }

        return app;
    }


    /// <summary>
    /// 读取配置
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    private static HangfireConfig GetConfig(
        this IConfiguration configuration)
    {
        return configuration.GetConfig<HangfireConfig>("HangfireConfig", "HANGFIRE_CONFIG");
    }
}