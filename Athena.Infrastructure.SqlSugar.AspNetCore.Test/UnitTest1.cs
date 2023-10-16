using SqlSugar;

namespace Athena.Infrastructure.SqlSugar.AspNetCore.Test;

public class UnitTest1 : TestBase
{
    [Fact]
    public void Test1()
    {
        Xunit.Assert.NotNull(GetService<ISqlSugarClient>());
    }
}