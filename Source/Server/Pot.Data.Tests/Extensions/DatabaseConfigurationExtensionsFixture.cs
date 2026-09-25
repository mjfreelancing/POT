using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pot.Data.Configuration;
using Pot.Data.Extensions;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Extensions;

public class DatabaseConfigurationExtensionsFixture : PotFixtureBase
{
    public class GetConnectionString : DatabaseConfigurationExtensionsFixture
    {
        [Fact]
        public void Should_Build_The_Connection_String_From_The_Configuration()
        {
            var configuration = new DatabaseConfiguration
            {
                Host = "localhost",
                Name = "Pot",
                Username = "postgres",
                Password = "secret",
                Port = 5444,
                SSLMode = "Prefer"
            };

            var result = configuration.GetConnectionString();

            result.ShouldBe("Host=localhost;Database=Pot;Username=postgres;Password=secret;Port=5444;SSLMode=Prefer");
        }
    }

    public class AddDatabaseConfiguration : DatabaseConfigurationExtensionsFixture
    {
        [Fact]
        public void Should_Resolve_The_Database_Configuration_From_The_Database_Section()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(BuildConfiguration(new Dictionary<string, string?>
            {
                ["Database:Host"] = "localhost",
                ["Database:Name"] = "Pot",
                ["Database:Username"] = "postgres",
                ["Database:Password"] = "secret",
                ["Database:Port"] = "5444",
                ["Database:SSLMode"] = "Prefer"
            }));

            services.AddDatabaseConfiguration();

            using var provider = services.BuildServiceProvider();

            var result = provider.GetRequiredService<DatabaseConfiguration>();

            result.Host.ShouldBe("localhost");
            result.Name.ShouldBe("Pot");
            result.Username.ShouldBe("postgres");
            result.Password.ShouldBe("secret");
            result.Port.ShouldBe(5444);
            result.SSLMode.ShouldBe("Prefer");
        }

        [Fact]
        public void Should_Throw_When_A_Required_Option_Is_Missing()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(BuildConfiguration(new Dictionary<string, string?>
            {
                ["Database:Name"] = "Pot",
                ["Database:Username"] = "postgres",
                ["Database:Password"] = "secret"
            }));

            services.AddDatabaseConfiguration();

            using var provider = services.BuildServiceProvider();

            var exception = Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<DatabaseConfiguration>());

            exception.Message.ShouldContain("Host");
        }
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
