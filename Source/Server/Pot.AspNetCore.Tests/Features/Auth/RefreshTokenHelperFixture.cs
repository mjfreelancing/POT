using Microsoft.AspNetCore.Http;
using Pot.AspNetCore.Concerns.Auth.Configuration;
using Pot.AspNetCore.Features.Auth;
using Pot.TestUtils;
using Shouldly;

namespace Pot.AspNetCore.Tests.Features.Auth;

public class RefreshTokenHelperFixture : PotFixtureBase
{
    private const string RefreshToken = "refresh-token-for-testing";
    private const string RefreshTokenCookieName = "pot_refresh_token";
    private const string CrossSubdomainCookieDomain = ".payontime.com.au";

    public class SetCookie : RefreshTokenHelperFixture
    {
        [Fact]
        public void Should_Not_Emit_Domain_Attribute_When_Configured_Domain_Is_Empty()
        {
            var setCookie = SetRefreshTokenCookie(domain: string.Empty);

            // An empty domain attribute is invalid and cookie stores reject the entire Set-Cookie header.
            setCookie.ShouldNotContain("domain=", Case.Insensitive);
        }

        [Fact]
        public void Should_Not_Emit_Domain_Attribute_When_Configured_Domain_Is_Null()
        {
            var setCookie = SetRefreshTokenCookie(domain: null);

            setCookie.ShouldNotContain("domain=", Case.Insensitive);
        }

        [Fact]
        public void Should_Emit_Domain_Attribute_When_Configured_Domain_Is_Provided()
        {
            var setCookie = SetRefreshTokenCookie(domain: CrossSubdomainCookieDomain);

            setCookie.ShouldContain($"domain={CrossSubdomainCookieDomain}");
        }

        [Fact]
        public void Should_Set_Refresh_Token_Value_And_Expected_Cookie_Attributes()
        {
            var setCookie = SetRefreshTokenCookie(domain: null);

            setCookie.ShouldContain($"{RefreshTokenCookieName}={RefreshToken}");
            setCookie.ShouldContain("httponly", Case.Insensitive);
            setCookie.ShouldContain("path=/", Case.Insensitive);
            setCookie.ShouldContain("samesite=lax", Case.Insensitive);
        }
    }

    public class ClearCookie : RefreshTokenHelperFixture
    {
        [Fact]
        public void Should_Emit_The_Same_Domain_Attribute_As_SetCookie()
        {
            var setCookie = SetRefreshTokenCookie(CrossSubdomainCookieDomain);
            var clearCookie = ClearRefreshTokenCookie(CrossSubdomainCookieDomain);

            setCookie.ShouldContain($"domain={CrossSubdomainCookieDomain}");
            clearCookie.ShouldContain($"domain={CrossSubdomainCookieDomain}");
        }

        [Fact]
        public void Should_Not_Emit_Domain_Attribute_When_Configured_Domain_Is_Empty()
        {
            var clearCookie = ClearRefreshTokenCookie(domain: string.Empty);

            clearCookie.ShouldNotContain("domain=", Case.Insensitive);
        }

        [Fact]
        public void Should_Emit_Empty_Value_And_Zero_Max_Age()
        {
            var clearCookie = ClearRefreshTokenCookie(domain: null);

            clearCookie.ShouldContain($"{RefreshTokenCookieName}=");
            clearCookie.ShouldContain("max-age=0", Case.Insensitive);
        }
    }

    private static AuthenticationOptions CreateAuthOptions(string? domain)
    {
        return new AuthenticationOptions
        {
            Cookie = new AuthenticationOptions.CookieOptions
            {
                SecureOnly = false,
                MaxAgeDays = 30,
                Domain = domain
            }
        };
    }

    private static string SetRefreshTokenCookie(string? domain)
    {
        var httpContext = new DefaultHttpContext();

        RefreshTokenHelper.SetCookie(httpContext, RefreshToken, CreateAuthOptions(domain));

        return httpContext.Response.Headers.SetCookie.ToString();
    }

    private static string ClearRefreshTokenCookie(string? domain)
    {
        var httpContext = new DefaultHttpContext();

        RefreshTokenHelper.ClearCookie(httpContext, CreateAuthOptions(domain));

        return httpContext.Response.Headers.SetCookie.ToString();
    }
}
