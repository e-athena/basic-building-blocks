namespace Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    public static IServiceCollection AddCustomMySqlDbContext<TDbContext>(
        this IServiceCollection services,
        string connectionString)
        where TDbContext : DbContext, IDbFacadeResolver
    {
        services.AddDbContext<TDbContext>(options =>
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(TDbContext).Assembly.GetName().Name);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                });
        });
        services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<TDbContext>()!);
        return services;
    }

    public static IServiceCollection AddCustomMySqlDbContext<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TDbContext : DbContext, IDbFacadeResolver
    {
        var connectionString = configuration.GetConnectionStringByEnv();
        services.AddDbContext<TDbContext>(options =>
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(TDbContext).Assembly.GetName().Name);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                });
        });
        services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<TDbContext>()!);
        return services;
    }


    /// <summary>
    /// 读取连接字符串
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns></returns>
    private static string GetConnectionStringByEnv(
        this IConfiguration configuration,
        string configVariable = "Default",
        string envVariable = "CONNECTION_STRING")
    {
        var connectionString = configuration.GetConnectionString(configVariable);
        var env = Environment.GetEnvironmentVariable(envVariable);
        if (!string.IsNullOrEmpty(env))
        {
            connectionString = env;
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        return connectionString;
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    /// <param name="migrationBuilder"></param>
    public static void MigrateDataFromScript(this MigrationBuilder migrationBuilder)
    {
        var assembly = Assembly.GetCallingAssembly();
        var files = assembly.GetManifestResourceNames();
        var filePrefix = $"{assembly.GetName().Name}.Data.Scripts."; //IMPORTANT

        var list = files
            .Where(f => f.StartsWith(filePrefix) && f.EndsWith(".sql"))
            .Select(f => new
            {
                PhysicalFile = f,
                LogicalFile = f.Replace(filePrefix, string.Empty)
            })
            .OrderBy(f => f.LogicalFile);

        foreach (var file in list)
        {
            using var stream = assembly.GetManifestResourceStream(file.PhysicalFile);
            using var reader = new StreamReader(stream!);
            var command = reader.ReadToEnd();

            if (string.IsNullOrWhiteSpace(command))
            {
                continue;
            }

            migrationBuilder.Sql(command);
        }
    }
}