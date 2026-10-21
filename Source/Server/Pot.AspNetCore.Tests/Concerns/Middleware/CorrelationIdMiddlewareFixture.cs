using Microsoft.AspNetCore.Http;
using NSubstitute;
using Pot.AspNetCore.Concerns.Middleware;
using Shouldly;
using System.Text.RegularExpressions;

namespace Pot.AspNetCore.Tests.Concerns.Middleware;

public class CorrelationIdMiddlewareFixture
{
    private readonly CorrelationIdMiddleware _middleware = new(Substitute.For<IProblemDetailsService>());

    public class InvokeAsync : CorrelationIdMiddlewareFixture
    {
        [Fact]
        public async Task Should_Serve_The_Request_When_There_Is_No_Activity()
        {
            // The hosting layer creates the request's Activity only when something observes it - a listener, or
            // its own diagnostics logger being enabled. Turning that logger off must not fail every request.
            var httpContext = CreateHttpContext("support-ticket-4821");
            var nextCalled = false;

            await _middleware.InvokeAsync(httpContext, _ =>
            {
                nextCalled = true;

                return Task.CompletedTask;
            });

            nextCalled.ShouldBeTrue();
            httpContext.TraceIdentifier.ShouldBe("support-ticket-4821");
        }

        [Fact]
        public async Task Should_Agree_With_The_Documented_Pattern_On_Every_Printable_Ascii_Character()
        {
            // The pattern describes the header in the OpenAPI document and the middleware never uses it, so nothing
            // else notices if one changes without the other. A client generated from a pattern looser than the
            // middleware sends ids that are then rejected.
            var disagreements = new List<char>();

            for (var character = '!'; character <= '~'; character++)
            {
                var correlationId = $"a{character}";
                var httpContext = CreateHttpContext(correlationId);

                await _middleware.InvokeAsync(httpContext, _ => Task.CompletedTask);

                var accepted = httpContext.Response.StatusCode != StatusCodes.Status400BadRequest;
                var matched = Regex.IsMatch(correlationId, CorrelationIdMiddleware.AllowedCharactersPattern);

                if (accepted != matched)
                {
                    disagreements.Add(character);
                }
            }

            disagreements.ShouldBeEmpty();
        }
    }

    private static DefaultHttpContext CreateHttpContext(string correlationId)
    {
        var httpContext = new DefaultHttpContext();

        httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = correlationId;

        return httpContext;
    }
}
