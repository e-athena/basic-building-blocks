namespace Athena.Infrastructure.EfCore;

/// <summary>
/// 
/// </summary>
public interface IDbFacadeResolver
{
    /// <summary>
    /// 
    /// </summary>
    DatabaseFacade Database { get; }
}