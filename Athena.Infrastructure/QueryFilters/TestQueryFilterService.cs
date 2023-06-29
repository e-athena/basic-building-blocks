namespace Athena.Infrastructure.QueryFilters;

/// <summary>
/// 
/// </summary>
public class TestQueryFilterService : IQueryFilterService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IList<QueryFilterGroup>?> GetAsync(string userId, Type type)
    {
        await Task.CompletedTask;
        return new List<QueryFilterGroup>
        {
            new()
            {
                XOR = "or",
                Filters = new List<QueryFilter>
                {
                    new()
                    {
                        Key = "PhoneNumber",
                        XOR = "and",
                        Operator = "contains",
                        Value = "1723"
                    },
                    new()
                    {
                        Key = "Password",
                        XOR = "and",
                        Operator = "contains",
                        Value = "4EIErJhVETg"
                    }
                }
            }
        };
    }
}