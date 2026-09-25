using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pot.Data.Extensions;
using Pot.Shared;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Extensions;

public class DbContextOptionsBuilderExtensionsFixture : PotFixtureBase
{
    public class ConfigurePostgres : DbContextOptionsBuilderExtensionsFixture
    {
        [Fact]
        public void Should_Configure_The_Npgsql_Provider()
        {
            var optionsBuilder = new DbContextOptionsBuilder<PotDbContext>();

            optionsBuilder.ConfigurePostgres("Host=localhost;Database=Pot;Username=postgres;Password=secret");

            using var dbContext = new PotDbContext(optionsBuilder.Options, Substitute.For<ICurrentUserContext>());

            dbContext.Database.ProviderName.ShouldBe("Npgsql.EntityFrameworkCore.PostgreSQL");
        }
    }
}
