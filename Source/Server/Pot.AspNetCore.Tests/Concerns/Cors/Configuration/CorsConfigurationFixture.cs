using Pot.AspNetCore.Concerns.Cors.Configuration;
using Shouldly;

namespace Pot.AspNetCore.Tests.Concerns.Cors.Configuration;

public class CorsConfigurationFixture
{
    [Fact]
    public void Should_Return_Origins_When_AllowedOrigins_Is_Comma_Separated()
    {
        var configuration = new CorsConfiguration
        {
            AllowedOrigins = "https://payontime.com.au,https://www.payontime.com.au"
        };

        var origins = configuration.GetAllowedOrigins();

        origins.Count.ShouldBe(2);
        origins.ShouldContain("https://payontime.com.au");
        origins.ShouldContain("https://www.payontime.com.au");
    }

    [Fact]
    public void Should_Return_Origins_When_AllowedOrigins_Is_Semicolon_Separated()
    {
        var configuration = new CorsConfiguration
        {
            AllowedOrigins = "https://payontime.com.au;https://www.payontime.com.au"
        };

        var origins = configuration.GetAllowedOrigins();

        origins.Count.ShouldBe(2);
        origins.ShouldContain("https://payontime.com.au");
        origins.ShouldContain("https://www.payontime.com.au");
    }

    [Fact]
    public void Should_Trim_Origins_And_Ignore_Empty_Segments()
    {
        var configuration = new CorsConfiguration
        {
            AllowedOrigins = "  https://payontime.com.au  , ;  https://www.payontime.com.au  ;; "
        };

        var origins = configuration.GetAllowedOrigins();

        origins.Count.ShouldBe(2);
        origins.ShouldContain("https://payontime.com.au");
        origins.ShouldContain("https://www.payontime.com.au");
    }

    [Fact]
    public void Should_Remove_Duplicate_Origins_Ignoring_Case()
    {
        var configuration = new CorsConfiguration
        {
            AllowedOrigins = "https://payontime.com.au,HTTPS://PAYONTIME.COM.AU;https://www.payontime.com.au"
        };

        var origins = configuration.GetAllowedOrigins();

        origins.Count.ShouldBe(2);
        origins.ShouldContain("https://payontime.com.au");
        origins.ShouldContain("https://www.payontime.com.au");
    }
}