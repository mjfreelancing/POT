using Pot.AspNetCore.Integration.Tests.Host;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Me;

/// <summary>
/// Proves a validation failure never returns the password the caller submitted.
/// </summary>
public class ChangePasswordFixture : IntegrationAuthFixtureBase
{
    private const string ChangePasswordPath = "/api/me/change-password";

    [Fact]
    public async Task Should_Not_Return_A_Weak_New_Password_In_The_Response()
    {
        var user = await CreateEnabledUserAsync("change-password-weak", "Change Password Weak");
        using var client = await CreateAuthenticatedClientAsync(user);

        var payload = new
        {
            CurrentPassword = user.Password,
            NewPassword = "weakpass1"
        };

        using var response = await client.PutAsJsonAsync(ChangePasswordPath, payload, TestContext.Current.CancellationToken);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        body.ShouldNotContain("weakpass1");
        body.ShouldNotContain("attemptedValue");
    }

    [Fact]
    public async Task Should_Not_Return_Either_Password_When_The_New_Password_Matches_The_Current()
    {
        var user = await CreateEnabledUserAsync("change-password-same", "Change Password Same");
        using var client = await CreateAuthenticatedClientAsync(user);

        var payload = new
        {
            CurrentPassword = user.Password,
            NewPassword = user.Password
        };

        using var response = await client.PutAsJsonAsync(ChangePasswordPath, payload, TestContext.Current.CancellationToken);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        body.ShouldContain("The new password must be different from the current password.");
        body.ShouldNotContain(user.Password);
        body.ShouldNotContain("attemptedValue");
    }
}
