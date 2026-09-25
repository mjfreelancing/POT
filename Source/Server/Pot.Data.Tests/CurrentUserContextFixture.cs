using Pot.TestUtils;
using Shouldly;
using System.Diagnostics;

namespace Pot.Data.Tests;

public class CurrentUserContextFixture : PotFixtureBase
{
    public class UserRowId : CurrentUserContextFixture
    {
        [Fact]
        public void Should_Throw_When_The_User_RowId_Has_Not_Been_Set()
        {
            var context = new CurrentUserContext();

            Should.Throw<UnreachableException>(() => _ = context.UserRowId);
        }

        [Fact]
        public void Should_Return_The_User_RowId_After_It_Has_Been_Set()
        {
            var rowId = Guid.NewGuid();
            var context = new CurrentUserContext();

            context.SetUserRowId(rowId);

            context.UserRowId.ShouldBe(rowId);
        }

        [Fact]
        public void Should_Return_The_Most_Recently_Set_User_RowId()
        {
            var context = new CurrentUserContext();
            context.SetUserRowId(Guid.NewGuid());

            var rowId = Guid.NewGuid();
            context.SetUserRowId(rowId);

            context.UserRowId.ShouldBe(rowId);
        }
    }
}
