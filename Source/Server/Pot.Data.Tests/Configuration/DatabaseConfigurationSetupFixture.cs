using Microsoft.Extensions.Configuration;
using Pot.Data.Configuration;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Configuration;

public class DatabaseConfigurationSetupFixture : PotFixtureBase
{
    public class Configure : DatabaseConfigurationSetupFixture
    {
        [Fact]
        public void Should_Bind_The_Database_Section()
        {
            var configuration = BuildConfiguration(new Dictionary<string, string?>
            {
                ["Database:Host"] = "localhost",
                ["Database:Name"] = "Pot",
                ["Database:Username"] = "postgres",
                ["Database:Password"] = "secret",
                ["Database:Port"] = "5444",
                ["Database:SSLMode"] = "Prefer"
            });

            var setup = new DatabaseConfigurationSetup(configuration);
            var options = UnboundConfiguration();

            setup.Configure(options);

            options.Host.ShouldBe("localhost");
            options.Name.ShouldBe("Pot");
            options.Username.ShouldBe("postgres");
            options.Password.ShouldBe("secret");
            options.Port.ShouldBe(5444);
            options.SSLMode.ShouldBe("Prefer");
        }
    }

    public class Validate : DatabaseConfigurationSetupFixture
    {
        [Fact]
        public void Should_Succeed_When_All_Required_Options_Are_Provided()
        {
            var setup = CreateSetup();

            var result = setup.Validate(name: null, CreateConfiguration());

            result.Succeeded.ShouldBeTrue();
        }

        [Fact]
        public void Should_Fail_When_The_Host_Is_Missing()
        {
            var setup = CreateSetup();

            var result = setup.Validate(name: null, CreateConfiguration(host: string.Empty));

            result.Failed.ShouldBeTrue();
            result.Failures.ShouldNotBeNull();
            result.Failures.Single().ShouldBe("Database configuration option 'Host' must be provided.");
        }

        [Fact]
        public void Should_Fail_When_The_Name_Is_Missing()
        {
            var setup = CreateSetup();

            var result = setup.Validate(name: null, CreateConfiguration(name: string.Empty));

            result.Failed.ShouldBeTrue();
            result.Failures.ShouldNotBeNull();
            result.Failures.Single().ShouldBe("Database configuration option 'Name' must be provided.");
        }

        [Fact]
        public void Should_Fail_When_The_Username_Is_Missing()
        {
            var setup = CreateSetup();

            var result = setup.Validate(name: null, CreateConfiguration(username: string.Empty));

            result.Failed.ShouldBeTrue();
            result.Failures.ShouldNotBeNull();
            result.Failures.Single().ShouldBe("Database configuration option 'Username' must be provided.");
        }

        [Fact]
        public void Should_Fail_When_The_Password_Is_Missing()
        {
            var setup = CreateSetup();

            var result = setup.Validate(name: null, CreateConfiguration(password: string.Empty));

            result.Failed.ShouldBeTrue();
            result.Failures.ShouldNotBeNull();
            result.Failures.Single().ShouldBe("Database configuration option 'Password' must be provided.");
        }

        [Fact]
        public void Should_Report_The_First_Missing_Option()
        {
            var setup = CreateSetup();

            var result = setup.Validate(name: null, CreateConfiguration(host: string.Empty, password: string.Empty));

            result.Failed.ShouldBeTrue();
            result.Failures.ShouldNotBeNull();
            result.Failures.Single().ShouldBe("Database configuration option 'Host' must be provided.");
        }
    }

    private static DatabaseConfigurationSetup CreateSetup()
    {
        return new DatabaseConfigurationSetup(BuildConfiguration(new Dictionary<string, string?>()));
    }

    private static DatabaseConfiguration CreateConfiguration(string host = "localhost", string name = "Pot",
        string username = "postgres", string password = "secret")
    {
        return new DatabaseConfiguration
        {
            Host = host,
            Name = name,
            Username = username,
            Password = password,
            Port = 5444,
            SSLMode = "Prefer"
        };
    }

    private static DatabaseConfiguration UnboundConfiguration()
    {
        return new DatabaseConfiguration
        {
            Host = string.Empty,
            Name = string.Empty,
            Username = string.Empty,
            Password = string.Empty,
            Port = 0,
            SSLMode = string.Empty
        };
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
