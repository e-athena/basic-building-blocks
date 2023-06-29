namespace Athena.Infrastructure.Domain.Commands;

/// <summary>
/// 兼容ENode的Command
/// </summary>
public class Command : ITxRequest<string>
{
    private string _aggregateRootId = string.Empty;

    /// <summary>
    /// Id
    /// </summary>
    public string AggregateRootId
    {
        get
        {
            if (_aggregateRootId == string.Empty && _id != string.Empty)
            {
                return _id;
            }

            return _aggregateRootId;
        }
        set => _aggregateRootId = value;
    }

    private string _id = string.Empty;

    /// <summary>
    /// Id
    /// </summary>
    public string Id
    {
        get
        {
            if (_id == string.Empty && _aggregateRootId != string.Empty)
            {
                return _aggregateRootId;
            }

            return _id;
        }
        set => _id = value;
    }

    /// <summary>
    /// 兼容ENode的Command
    /// </summary>
    public Command()
    {
        AggregateRootId = string.Empty;
        Id = string.Empty;
    }

    /// <summary>
    /// 兼容ENode的Command
    /// </summary>
    /// <param name="id">ID</param>
    public Command(string id)
    {
        AggregateRootId = id;
        Id = id;
    }
}