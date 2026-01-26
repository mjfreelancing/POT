using FluentAssertions;
using Pot.Data.Entities;
using Pot.Data.Specifications;
using Pot.TestUtils;

namespace Pot.Data.Tests.Specifications;

public class EntitySpecificationsFixture : PotFixtureBase
{
    private readonly SiteEntity _site;

    public EntitySpecificationsFixture()
    {
        _site = EntityFactory.CreateSite();
    }

    public class IsSameId : EntitySpecificationsFixture
    {
        [Fact]
        public void Should_Return_True_When_RowId_Matches()
        {
            var rowId = Guid.NewGuid();
            var account = EntityFactory.CreateAccount(_site, "Test Account", 0.0d);
            account.RowId = rowId;

            var specification = EntitySpecifications.IsSameId<AccountEntity>(rowId);
            var result = specification.IsSatisfiedBy(account);

            result.Should().BeTrue();
        }

        [Fact]
        public void Should_Return_False_When_RowId_Does_Not_Match()
        {
            var rowId = Guid.NewGuid();
            var differentRowId = Guid.NewGuid();
            var account = EntityFactory.CreateAccount(_site, "Test Account", 0.0d);
            account.RowId = differentRowId;

            var specification = EntitySpecifications.IsSameId<AccountEntity>(rowId);
            var result = specification.IsSatisfiedBy(account);

            result.Should().BeFalse();
        }

        [Fact]
        public void Should_Work_With_Different_Entity_Types()
        {
            var rowId = Guid.NewGuid();

            var site = EntityFactory.CreateSite();
            site.RowId = rowId;

            var specification = EntitySpecifications.IsSameId<SiteEntity>(rowId);
            var result = specification.IsSatisfiedBy(site);

            result.Should().BeTrue();
        }
    }
}
