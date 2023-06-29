namespace Athena.Infrastructure.EfCore;

public abstract class DbContextDesignFactoryBase<TDbContext> :
    IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    public TDbContext CreateDbContext(string[] args)
    {
        var connectionStringName = "Default";
        if (args.Length == 1)
        {
            connectionStringName = args[0];
        }

        var config = new ConfigurationBuilder()
            .AddJsonFile("config.json")
            .Build();

        var connectionString = config?.GetConnectionString(connectionStringName);

        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        // MySql
        optionsBuilder.UseMySql(
            connectionString ?? throw new InvalidOperationException(),
            ServerVersion.AutoDetect(connectionString),
            sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(GetType().Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
            }
        );

        return (TDbContext) Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;
    }
}