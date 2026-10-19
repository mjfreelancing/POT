using Microsoft.AspNetCore.Cors.Infrastructure;
using Pot.AspNetCore.Concerns.Cors.Configuration;
using Shouldly;

namespace Pot.AspNetCore.Tests.Concerns.Cors.Configuration;

public class CorsOptionsSetupFixture
{
    public class Configure : CorsOptionsSetupFixture
    {
        [Fact]
        public void Should_Register_The_Policy_As_The_Default()
        {
            var options = ConfigureOptions("https://app.example.com");

            options.GetPolicy(options.DefaultPolicyName).ShouldNotBeNull();
        }

        [Fact]
        public void Should_Put_The_Configured_Origins_On_The_Policy()
        {
            var policy = GetDefaultPolicy("https://one.example.com;https://two.example.com");

            policy.Origins.ShouldBe(["https://one.example.com", "https://two.example.com"]);
        }

        [Fact]
        public void Should_Allow_Any_Method_And_Header()
        {
            var policy = GetDefaultPolicy("https://app.example.com");

            policy.AllowAnyMethod.ShouldBeTrue();
            policy.AllowAnyHeader.ShouldBeTrue();
        }

        [Fact]
        public void Should_Support_Credentials()
        {
            var policy = GetDefaultPolicy("https://app.example.com");

            policy.SupportsCredentials.ShouldBeTrue();
        }

        [Fact]
        public void Should_Expose_Content_Disposition_And_Retry_After()
        {
            var policy = GetDefaultPolicy("https://app.example.com");

            policy.ExposedHeaders.ShouldBe(["content-disposition", "retry-after"], ignoreOrder: true);
        }

        [Fact]
        public void Should_Cache_The_Preflight_For_Ten_Minutes()
        {
            var policy = GetDefaultPolicy("https://app.example.com");

            policy.PreflightMaxAge.ShouldBe(TimeSpan.FromMinutes(10));
        }
    }

    private static CorsOptions ConfigureOptions(string allowedOrigins)
    {
        var configuration = new CorsConfiguration { AllowedOrigins = allowedOrigins };
        var setup = new CorsOptionsSetup(configuration);
        var options = new CorsOptions();

        setup.Configure(options);

        return options;
    }

    private static CorsPolicy GetDefaultPolicy(string allowedOrigins)
    {
        var options = ConfigureOptions(allowedOrigins);
        var policy = options.GetPolicy(options.DefaultPolicyName);

        policy.ShouldNotBeNull();

        return policy;
    }
}
