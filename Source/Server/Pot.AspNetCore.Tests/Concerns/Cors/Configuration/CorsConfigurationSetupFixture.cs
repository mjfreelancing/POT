using Microsoft.Extensions.Configuration;
using Pot.AspNetCore.Concerns.Cors.Configuration;
using Shouldly;

namespace Pot.AspNetCore.Tests.Concerns.Cors.Configuration;

public class CorsConfigurationSetupFixture
{
    public class Configure : CorsConfigurationSetupFixture
    {
        [Fact]
        public void Should_Bind_AllowedOrigins_From_The_Cors_Section()
        {
            var setup = CreateSetup(("Cors:AllowedOrigins", "https://app.example.com"));
            var options = CreateUnbound();

            setup.Configure(options);

            options.AllowedOrigins.ShouldBe("https://app.example.com");
        }

        [Fact]
        public void Should_Leave_AllowedOrigins_Unchanged_When_The_Section_Is_Absent()
        {
            var setup = CreateSetup(("Database:Host", "localhost"));
            var options = CreateUnbound();

            setup.Configure(options);

            options.AllowedOrigins.ShouldBe("not-yet-bound");
        }
    }

    public class Validate : CorsConfigurationSetupFixture
    {
        [Theory]
        [InlineData("https://app.example.com")]
        [InlineData("http://localhost:5175")]
        [InlineData("https://app.example.com:8443")]
        [InlineData("https://openapi-build.invalid")]
        [InlineData("https://one.example.com,http://two.example.com;https://three.example.com")]
        public void Should_Accept_Valid_Origins(string allowedOrigins)
        {
            var result = ValidateOrigins(allowedOrigins);

            result.Succeeded.ShouldBeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(",")]
        [InlineData(" ; , ")]
        public void Should_Fail_When_No_Origin_Is_Provided(string allowedOrigins)
        {
            var result = ValidateOrigins(allowedOrigins);

            result.Failed.ShouldBeTrue();
            result.FailureMessage.ShouldBe("Cors option 'AllowedOrigins' must be provided.");
        }

        [Theory]
        [InlineData("*")]
        [InlineData("https://app.example.com/")]
        [InlineData("https://app.example.com/app")]
        [InlineData("https://app.example.com?x=1")]
        [InlineData("https://app.example.com#x")]
        [InlineData("app.example.com")]
        [InlineData("ftp://app.example.com")]
        [InlineData("https://user@example.com")]
        public void Should_Fail_When_An_Entry_Is_Not_An_Origin(string origin)
        {
            var result = ValidateOrigins(origin);

            result.Failed.ShouldBeTrue();
            result.FailureMessage.ShouldContain(origin);
            result.FailureMessage.ShouldContain("absolute http or https origin");
        }
    }

    private static CorsConfigurationSetup CreateSetup(params (string Key, string? Value)[] values)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(item => new KeyValuePair<string, string?>(item.Key, item.Value)))
            .Build();

        return new CorsConfigurationSetup(configuration);
    }

    private static CorsConfiguration CreateUnbound()
    {
        // The options system creates the instance before Configure() is called, so the required member
        // has a placeholder until binding replaces it.
        return new CorsConfiguration { AllowedOrigins = "not-yet-bound" };
    }

    private static Microsoft.Extensions.Options.ValidateOptionsResult ValidateOrigins(string allowedOrigins)
    {
        var setup = CreateSetup();
        var options = new CorsConfiguration { AllowedOrigins = allowedOrigins };

        return setup.Validate(name: null, options);
    }
}
