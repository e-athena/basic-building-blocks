namespace Athena.Infrastructure.Domain;

/// <summary>
/// 全功能接口
/// </summary>
public interface IFullCore : ICreator, IUpdater, ISoftDelete, IOrganization, ITenant
{
}