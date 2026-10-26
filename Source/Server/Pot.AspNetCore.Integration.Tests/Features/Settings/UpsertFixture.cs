using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Pot.AspNetCore.Integration.Tests.Features.Settings;

/// <summary>
/// Proves a settings validation failure never returns the value the caller put in the route, and that an upsert
/// carrying an entity tag the site no longer holds is refused as a stale conflict.
/// </summary>
public class UpsertFixture : IntegrationAuthFixtureBase
{
    private const string Category = "EmailBudgetReminder";
    private const string Key = "ReminderDays";
    private const string StaleEtagMessage = "a stale entity tag must not be accepted";
    private const string StaleEtagError = "The entity tag for the Setting does not match the current record";
    [Fact]
    public async Task Should_Not_Echo_The_Submitted_Category_In_A_Validation_Message()
    {
        var user = await CreateAdminUserAsync("settings-category", "Settings Category");
        using var client = await CreateAuthenticatedClientAsync(user);

        const string category = "not-a-category";
        var payload = new
        {
            Value = "1"
        };

        using var response = await client.PutAsJsonAsync($"/api/settings/{category}/anything", payload,
            TestContext.Current.CancellationToken);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // 'instance' repeats the request path, which the caller already supplied, so only the message fields are
        // checked: the rejected category is not echoed back in any of them.
        using var document = JsonDocument.Parse(body);
        var problem = document.RootElement;

        problem.TryGetProperty("detail", out _).ShouldBeFalse();

        var errors = problem.GetProperty("errors");

        errors.ValueKind.ShouldBe(JsonValueKind.Object);

        var messages = errors.GetProperty("category").EnumerateArray()
            .Select(message => message.GetString() ?? string.Empty)
            .ToArray();

        var message = messages.ShouldHaveSingleItem();

        message.ShouldNotContain(category);
    }

    [Fact]
    public async Task Should_Return_Conflict_When_The_Etag_Is_Stale()
    {
        var user = await CreateAdminUserAsync("settings-stale", "Settings Stale User");
        using var client = await CreateAuthenticatedClientAsync(user);

        // A site starts with the category defaults, which carry no entity tag until one is saved.
        using var created = await client.PutAsJsonAsync($"/api/settings/{Category}/{Key}",
            new { Value = "3", Etag = (long?)null }, TestContext.Current.CancellationToken);

        await created.ShouldHaveStatusAsync(HttpStatusCode.OK);

        var settings = await client.GetSettingsAsync();
        var setting = settings.Settings.Single(item => item.Category == Category && item.Key == Key);

        setting.Etag.ShouldNotBeNull();

        using var response = await client.PutAsJsonAsync($"/api/settings/{Category}/{Key}",
            new { Value = "4", Etag = setting.Etag - 1 }, TestContext.Current.CancellationToken);

        await response.ShouldBeStaleEtagConflictAsync(StaleEtagError, StaleEtagMessage);
    }
}
