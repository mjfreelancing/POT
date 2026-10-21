using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Pot.RazorComponents;
using Pot.RazorComponents.Emails.ApprovalAccepted;
using Pot.RazorComponents.Emails.ApprovalRejected;
using Pot.RazorComponents.Emails.Signup;
using Pot.RazorComponents.Emails.SiteDeleted;
using Pot.RazorComponents.Emails.UserDeleted;
using Pot.RazorComponents.Extensions;
using Pot.RazorComponents.Models;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Emails;

/// <summary>
/// Renders the email templates with the values the sender passes to them, so a template that addresses the
/// recipient by the wrong value is caught even when the model carries the right one.
/// </summary>
public class EmailGreetingFixture : PotFixtureBase
{
    private const string Username = "jsmith";
    private const string DisplayName = "Jane Smith";

    private static async Task<string> RenderHtmlAsync<TComponent>(IDictionary<string, object?> parameters)
        where TComponent : IComponent
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddRazorComponentRendering();

        await using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<IRazorComponentRenderer>();

        return await renderer.RenderToHtmlAsync<TComponent>(parameters);
    }

    private static string RenderPlainText(string templateName, IDictionary<string, object?> replacements)
    {
        return PlainTextEmailTemplateLoader.Populate(templateName, replacements);
    }

    public class ApprovalAcceptedGreeting : EmailGreetingFixture
    {
        [Fact]
        public async Task Should_Greet_The_User_By_Display_Name_In_The_Html()
        {
            var html = await RenderHtmlAsync<ApprovalAcceptedEmail>(new Dictionary<string, object?>
            {
                ["Username"] = Username,
                ["DisplayName"] = DisplayName
            });

            html.ShouldContain("Hello Jane Smith,");
            html.ShouldNotContain("Hello jsmith,");
        }

        [Fact]
        public void Should_Greet_The_User_By_Display_Name_In_The_Plain_Text()
        {
            var plainText = RenderPlainText(PlainTextEmailTemplateLoader.ApprovalAccepted, new Dictionary<string, object?>
            {
                ["Username"] = Username,
                ["DisplayName"] = DisplayName
            });

            plainText.ShouldContain("Hello Jane Smith,");
            plainText.ShouldNotContain("Hello jsmith,");
        }
    }

    public class ApprovalRejectedGreeting : EmailGreetingFixture
    {
        [Fact]
        public async Task Should_Greet_The_User_By_Display_Name_In_The_Html()
        {
            var html = await RenderHtmlAsync<ApprovalRejectedEmail>(new Dictionary<string, object?>
            {
                ["Username"] = Username,
                ["DisplayName"] = DisplayName
            });

            html.ShouldContain("Hello Jane Smith,");
            html.ShouldNotContain("Hello jsmith,");
        }

        [Fact]
        public void Should_Greet_The_User_By_Display_Name_In_The_Plain_Text()
        {
            var plainText = RenderPlainText(PlainTextEmailTemplateLoader.ApprovalRejected, new Dictionary<string, object?>
            {
                ["Username"] = Username,
                ["DisplayName"] = DisplayName
            });

            plainText.ShouldContain("Hello Jane Smith,");
            plainText.ShouldNotContain("Hello jsmith,");
        }
    }

    public class SignupGreeting : EmailGreetingFixture
    {
        // A signup has not created a user yet, so there is no display name to greet the signer by. The username is
        // the only name the request carries.
        [Fact]
        public async Task Should_Greet_The_Signer_By_Username_In_The_Html()
        {
            var html = await RenderHtmlAsync<SignupEmail>(new Dictionary<string, object?>
            {
                ["Username"] = Username,
                ["ReferenceCode"] = "ref-123",
                ["VerificationCode"] = "123456",
                ["TempPassword"] = "temp-pass",
                ["OtpExpiryMinutes"] = 15
            });

            html.ShouldContain("Hello jsmith,");
        }

        [Fact]
        public void Should_Greet_The_Signer_By_Username_In_The_Plain_Text()
        {
            var plainText = RenderPlainText(PlainTextEmailTemplateLoader.Signup, new Dictionary<string, object?>
            {
                ["Username"] = Username,
                ["ReferenceCode"] = "ref-123",
                ["VerificationCode"] = "123456",
                ["TempPassword"] = "temp-pass",
                ["OtpExpiryMinutes"] = 15
            });

            plainText.ShouldContain("Hello jsmith,");
        }
    }

    public class UserDeletedGreeting : EmailGreetingFixture
    {
        [Fact]
        public async Task Should_Greet_The_Recipient_By_Display_Name_In_The_Html()
        {
            var html = await RenderHtmlAsync<UserDeletedEmail>(new Dictionary<string, object?>
            {
                ["Username"] = Username,
                ["DisplayName"] = DisplayName,
                ["Heading"] = "User Deleted",
                ["Introduction"] = "A user account on your site has been deleted.",
                ["Details"] = new List<EmailUserDeletedInfo.DetailInfo>
                {
                    new() { Label = "User", Value = "Taylor Target (target)" },
                    new() { Label = "Deleted by", Value = "Casey Caller (caller)" }
                },
                ["ClosingNote"] = "This is an automated confirmation. No further action is required."
            });

            html.ShouldContain("Hello Jane Smith,");
            html.ShouldNotContain("Hello jsmith,");
            html.ShouldContain("Taylor Target (target)");
            html.ShouldContain("Casey Caller (caller)");
        }

        [Fact]
        public void Should_Greet_The_Recipient_By_Display_Name_In_The_Plain_Text()
        {
            var plainText = RenderPlainText(PlainTextEmailTemplateLoader.UserDeleted, new Dictionary<string, object?>
            {
                ["Username"] = Username,
                ["DisplayName"] = DisplayName,
                ["Heading"] = "User Deleted",
                ["HeadingUnderline"] = "=============",
                ["Introduction"] = "A user account on your site has been deleted.",
                ["DetailsSection"] = "User:       Taylor Target (target)",
                ["ClosingNote"] = "This is an automated confirmation. No further action is required."
            });

            plainText.ShouldContain("Hello Jane Smith,");
            plainText.ShouldNotContain("Hello jsmith,");
            plainText.ShouldContain("Taylor Target (target)");
        }
    }

    public class SiteDeletedGreeting : EmailGreetingFixture
    {
        [Fact]
        public async Task Should_Greet_The_Administrator_By_Display_Name_In_The_Html()
        {
            var html = await RenderHtmlAsync<SiteDeletedEmail>(new Dictionary<string, object?>
            {
                ["Username"] = Username,
                ["DisplayName"] = DisplayName,
                ["DeletedSiteName"] = "Delete Site",
                ["DeletedByLabel"] = "Casey Caller (caller)",
                ["DeletedAtLabel"] = "Wed, 21 Oct 2026, 2:30 PM"
            });

            html.ShouldContain("Hello Jane Smith,");
            html.ShouldNotContain("Hello jsmith,");
            html.ShouldContain("Your site has been deleted, together with its users and all of its data.");
            html.ShouldContain("Casey Caller (caller)");
        }

        [Fact]
        public void Should_Greet_The_Administrator_By_Display_Name_In_The_Plain_Text()
        {
            var plainText = RenderPlainText(PlainTextEmailTemplateLoader.SiteDeleted, new Dictionary<string, object?>
            {
                ["Username"] = Username,
                ["DisplayName"] = DisplayName,
                ["DeletedSiteName"] = "Delete Site",
                ["DeletedByLabel"] = "Casey Caller (caller)",
                ["DeletedAtLabel"] = "Wed, 21 Oct 2026, 2:30 PM"
            });

            plainText.ShouldContain("Hello Jane Smith,");
            plainText.ShouldNotContain("Hello jsmith,");
            plainText.ShouldContain("Your site has been deleted, together with its users and all of its data.");
            plainText.ShouldContain("Casey Caller (caller)");
        }
    }
}
