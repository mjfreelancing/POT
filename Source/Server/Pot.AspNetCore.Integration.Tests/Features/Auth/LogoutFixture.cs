using Pot.AspNetCore.Integration.Tests.Host;
using Pot.AspNetCore.Integration.Tests.Host.Extensions;
using Pot.TestUtils;
using Pot.TestUtils.Logging;
using Shouldly;
using System.Net;

namespace Pot.AspNetCore.Integration.Tests.Features.Auth;

public class LogoutFixture : IClassFixture<ProductionApiWebApplicationFactory>
{
    private const string SetCookieHeader = "Set-Cookie";
    private const string RefreshTokenCookieName = "pot_refresh_token";
    private const string LogoutHandlerLogCategory = "Pot.AspNetCore.Features.Auth.Logout.Handler";
    private const string LogoutHandlerLogMessage = "Call: Invoke";
    private const string MethodNameProperty = "MethodName";
    private const string OriginalFormatProperty = "{OriginalFormat}";
    private const string CorrelationIdProperty = "correlationId";
    private const string LogoutCallTemplate = "Call: {MethodName}";

    private readonly ProductionApiWebApplicationFactory _factory;

    public LogoutFixture(ProductionApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_Return_MethodNotAllowed_When_Getting_Logout_Endpoint()
    {
        using var client = _factory.CreateClient();

        var actual = await client.GetAsync("/api/auth/logout");

        actual.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Should_Return_Ok_When_Posting_Logout_Endpoint_Anonymously()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/auth/logout", null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Clear_RefreshToken_Cookie_When_Posting_Logout_Endpoint()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/auth/logout", null);
        var setCookieValues = response.ShouldHaveHeaderValues(SetCookieHeader);

        setCookieValues.ShouldContainValue($"{RefreshTokenCookieName}=", StringComparison.Ordinal);
        setCookieValues.ShouldContainValue("Max-Age=0", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_Write_Logout_Handler_Log_When_Posting_Logout_Endpoint()
    {
        var collector = _factory.Services.GetFakeLogCollector();
        collector.Clear();

        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/auth/logout", null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var actual = collector.ShouldContainLogCall(
            LogoutHandlerLogCategory,
            callerName: "Invoke",
            callerType: null,
            assertNoAdditionalStructuredState: false);

        actual.ShouldContainStructuredStateStringValue(MethodNameProperty, "Invoke")
            .ShouldContainStructuredStateStringValue(OriginalFormatProperty, LogoutCallTemplate)
            .ShouldContainStructuredStateNonEmptyStringValue(CorrelationIdProperty);

        actual.Message.ShouldBe(LogoutHandlerLogMessage);
    }
}