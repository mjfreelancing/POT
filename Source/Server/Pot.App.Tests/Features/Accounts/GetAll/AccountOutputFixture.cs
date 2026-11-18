using Pot.App.Features.Accounts.GetAll.Models;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Accounts.GetAll;

public class AccountOutputFixture : PotFixtureBase
{
    [Fact]
    public void Should_Derive_Available_From_The_Rounded_Components()
    {
        // 0.3 - 0.1 - 0.1 is 0.09999999999999998 in binary arithmetic, while the figure a reader sees is 0.10.
        var output = new Output
        {
            RowId = Guid.NewGuid(),
            Etag = 1L,
            Description = "Main account",
            Balance = 0.3d,
            Reserved = 0.1d,
            TotalExpenseAccrued = 0.0d,
            TotalArrears = 0.0d,
            TotalCommitted = 0.1d,
            StableExpenseAccrual = 0.0d,
            LinkedExpenses = 0,
            LinkedIncomes = 0
        };

        output.Available.ShouldBe(0.1d);
    }
}
