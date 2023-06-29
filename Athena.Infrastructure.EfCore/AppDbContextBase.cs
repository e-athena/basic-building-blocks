namespace Athena.Infrastructure.EfCore;

public abstract class AppDbContextBase<TKey> : DbContext, IDbFacadeResolver
{
    protected AppDbContextBase(DbContextOptions options) : base(options)
    {
    }
}