namespace Athena.Infrastructure.Lucene.Interfaces;

public interface IEntity<TPrimaryKey>
{
    TPrimaryKey Id { get; set; }
}

public interface IEntity : IEntity<string>
{
}