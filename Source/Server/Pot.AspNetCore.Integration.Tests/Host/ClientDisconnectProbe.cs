using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Pot.AspNetCore.Integration.Tests.Host;

/// <summary>
/// Simulates a request whose client has disconnected, so the framework's client-disconnect handling can
/// be exercised through the real pipeline.
/// </summary>
/// <remarks>
/// The connection stays open, so the test can read the 499 the exception handler middleware records.
/// The probe is terminal middleware, and the application's fallback authorization policy requires an
/// authenticated user for a request that matches no endpoint, so a test principal is supplied before
/// authorization runs.
/// </remarks>
internal sealed class ClientDisconnectProbe : IStartupFilter
{
    public const string Path = "/__client-disconnect-probe";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.Use(async (context, nextMiddleware) =>
            {
                context.User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim("sub", Guid.NewGuid().ToString())],
                    authenticationType: "Test"));

                await nextMiddleware(context);
            });

            next(app);

            app.Run(context =>
            {
                if (context.Request.Path != Path)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;

                    return Task.CompletedTask;
                }

                var scenario = context.Request.Query["scenario"].ToString();

                if (scenario == "server-canceled")
                {
                    // A cancellation from server-side work. The client is still connected, so
                    // RequestAborted is not cancelled and the framework's client-disconnect path does
                    // not apply: the exception handlers are called and the request ends as a 500.
                    throw new OperationCanceledException("The request was cancelled by the server.");
                }

                // Simulate the framework observing a client disconnect. Cancelling the real connection
                // would abort the response the test needs to read.
                var aborted = new CancellationTokenSource();
                aborted.Cancel();
                context.RequestAborted = aborted.Token;

                throw scenario == "io"
                    ? new IOException("The client disconnected.")
                    : new OperationCanceledException(context.RequestAborted);
            });
        };
    }
}
