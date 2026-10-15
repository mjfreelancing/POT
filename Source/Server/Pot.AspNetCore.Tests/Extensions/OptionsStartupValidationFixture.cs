using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pot.AspNetCore.Extensions;
using Shouldly;

namespace Pot.AspNetCore.Tests.Extensions;

public class OptionsStartupValidationFixture
{
    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public void Should_Validate_Registered_Options_On_Startup(string environmentName)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = [],
            EnvironmentName = environmentName
        });

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = string.Empty,
            ["Jwt:Audience"] = "pot-tests",
            ["Jwt:SecretKey"] = "pot-tests-secret",
            ["PlatformAdmin:UserIds"] = "not-a-guid",
            ["Cors:AllowedOrigins"] = string.Empty,
            ["Smtp:Host"] = string.Empty,
            ["Smtp:Port"] = "465",
            ["Smtp:RequireTls"] = "true",
            ["Smtp:Authentication:Username"] = "pot-tests",
            ["Smtp:Authentication:Password"] = "pot-tests-secret",
            ["Smtp:From:Name"] = "POT Tests",
            ["Smtp:From:Address"] = "pot-tests@example.com"
        });

        builder
            .AddPotAuth()
            .AddPotCors()
            .AddSmtp();

        using var provider = builder.Services.BuildServiceProvider();
        var startupValidator = provider.GetRequiredService<IStartupValidator>();

        var exception = Should.Throw<AggregateException>(() => startupValidator.Validate());
        var validationMessages = exception.Flatten().InnerExceptions.Select(item => item.Message);

        validationMessages.ShouldContain(message => message.Contains("Issuer", StringComparison.Ordinal));
        validationMessages.ShouldContain(message => message.Contains("PlatformAdmin", StringComparison.Ordinal));
        validationMessages.ShouldContain(message => message.Contains("Cors", StringComparison.Ordinal));
        validationMessages.ShouldContain(message => message.Contains("Smtp", StringComparison.Ordinal));
    }
}
