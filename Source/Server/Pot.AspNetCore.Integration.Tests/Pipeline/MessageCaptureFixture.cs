using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.EmailSender;
using Pot.RazorComponents.Models;
using Pot.Shared.Enumerations;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Pot.AspNetCore.Integration.Tests.Pipeline;

/// <summary>
/// Proves that a test can read the messages the application submits, and that the shipped application cannot
/// record one.
/// </summary>
public class MessageCaptureFixture : IntegrationAuthFixtureBase
{
    private sealed class RoleOption
    {
        public Guid RowId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task Should_Capture_The_Message_An_Operation_Submits()
    {
        var caller = await CreateAdminUserAsync("message-capture-caller", "Message Capture Caller");

        using var client = await CreateAuthenticatedClientAsync(caller);

        var username = $"message-capture-{Guid.NewGuid():N}";
        var email = $"{username}@example.com";

        using (var response = await SendInviteAsync(client, username, email))
        {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        // Read once the operation has returned, which is what makes the message actionable: a test never waits.
        var captured = Emails.Latest(EmailType.Invitation, email);

        captured.ShouldNotBeNull("the message the operation submitted must be readable afterwards");
        captured.Type.ShouldBe(EmailType.Invitation);

        var invitation = captured.Content.ShouldBeOfType<EmailInvitationInfo>();

        invitation.Username.ShouldBe(username);
        invitation.TempPassword.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Yield_An_Invitations_Password_Through_One_Helper()
    {
        var caller = await CreateAdminUserAsync("message-password-caller", "Message Password Caller");

        using var client = await CreateAuthenticatedClientAsync(caller);

        var username = $"message-password-{Guid.NewGuid():N}";
        var email = $"{username}@example.com";

        using (var response = await SendInviteAsync(client, username, email))
        {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        var password = Emails.GetInvitationPassword(email);

        password.ShouldNotBeNullOrWhiteSpace();

        // The helper is the only reader: it returns the secret the message carries, not a copy of its wording.
        Emails.Latest(EmailType.Invitation, email)!
            .Content.ShouldBeOfType<EmailInvitationInfo>()
            .TempPassword.ShouldBe(password);
    }

    [Fact]
    public void Should_Not_Ship_A_Way_To_Record_Messages()
    {
        var shippedWriters = typeof(ISendEmailChannelWriter).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(ISendEmailChannelWriter).IsAssignableFrom(type))
            .ToList();

        shippedWriters.ShouldHaveSingleItem("the shipped assembly offers exactly one writer");

        typeof(CapturingEmailWriter).Assembly.ShouldNotBe(
            typeof(ISendEmailChannelWriter).Assembly,
            "the capture lives in the tests, so no configuration can make the product record a message");
    }

    private async Task<HttpResponseMessage> SendInviteAsync(HttpClient client, string username, string email)
    {
        var roles = await client.GetFromJsonAsync<RoleOption[]>("/api/roles", TestContext.Current.CancellationToken);

        var viewerRoleId = roles!.Single(role => role.Name == Role.Viewer.Name).RowId;

        var request = new
        {
            Username = username,
            Email = email,
            RoleIds = new[] { viewerRoleId }
        };

        return await client.PostAsJsonAsync("/api/users/invite", request, TestContext.Current.CancellationToken);
    }
}
