using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Pot.AspNetCore.Concerns.Auth.Configuration;
using Pot.AspNetCore.Concerns.Auth.Models;
using Shouldly;

namespace Pot.AspNetCore.Tests.Concerns.Auth.Configuration;

public class JwtOptionsSetupFixture
{
    [Fact]
    public void Should_Return_Failed_Validation_When_Issuer_Is_Missing()
    {
        var setup = CreateSetup(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = string.Empty,
            ["Jwt:Audience"] = "pot-tests",
            ["Jwt:SecretKey"] = "pot-tests-secret"
        });

        var options = new JwtOptions
        {
            Issuer = string.Empty,
            Audience = "pot-tests",
            SecretKey = "pot-tests-secret"
        };

        var result = setup.Validate(null, options);

        result.Failed.ShouldBeTrue();
        result.FailureMessage.ShouldContain("Issuer");
    }

    [Fact]
    public void Should_Configure_And_Validate_Successfully_When_Jwt_Values_Are_Present()
    {
        var setup = CreateSetup(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "pot-tests",
            ["Jwt:Audience"] = "pot-tests",
            ["Jwt:SecretKey"] = "pot-tests-secret"
        });

        var options = new JwtOptions
        {
            Issuer = string.Empty,
            Audience = string.Empty,
            SecretKey = string.Empty
        };

        setup.Configure(options);
        var result = setup.Validate(null, options);

        result.ShouldBe(ValidateOptionsResult.Success);
        options.Issuer.ShouldBe("pot-tests");
        options.Audience.ShouldBe("pot-tests");
        options.SecretKey.ShouldBe("pot-tests-secret");
    }

    private static JwtOptionsSetup CreateSetup(IDictionary<string, string?> values)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new JwtOptionsSetup(configuration);
    }
}
