using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Pot.AspNetCore.Concerns.Middleware;
using Shouldly;

namespace Pot.AspNetCore.Tests.Concerns.Middleware;

public class SecurityHeadersMiddlewareFixture
{
    private readonly SecurityHeadersMiddleware _middleware = new();
    private readonly StartableResponseFeature _responseFeature = new();
    private readonly DefaultHttpContext _httpContext = new();

    public SecurityHeadersMiddlewareFixture()
    {
        // DefaultHttpContext's own response feature discards OnStarting callbacks, and the middleware does all of its
        // work in one, so the response has to be startable for anything here to be observable.
        _httpContext.Features.Set<IHttpResponseFeature>(_responseFeature);
    }

    public class InvokeAsync : SecurityHeadersMiddlewareFixture
    {
        [Fact]
        public async Task Should_Add_NoSniff()
        {
            await _middleware.InvokeAsync(_httpContext, _ => Task.CompletedTask);
            await _responseFeature.StartAsync();

            _httpContext.Response.Headers.XContentTypeOptions.ToString().ShouldBe(SecurityHeadersMiddleware.NoSniff);
        }

        [Fact]
        public async Task Should_Add_NoStore_When_Nothing_Chose_A_Cache_Policy()
        {
            await _middleware.InvokeAsync(_httpContext, _ => Task.CompletedTask);
            await _responseFeature.StartAsync();

            _httpContext.Response.Headers.CacheControl.ToString().ShouldBe(SecurityHeadersMiddleware.NoStore);
        }

        [Fact]
        public async Task Should_Leave_A_Cache_Policy_The_Endpoint_Chose()
        {
            // The opt-out. An endpoint serving genuinely cacheable data says so itself, where a reviewer can see it.
            await _middleware.InvokeAsync(_httpContext, context =>
            {
                context.Response.Headers.CacheControl = "public, max-age=60";

                return Task.CompletedTask;
            });

            await _responseFeature.StartAsync();

            _httpContext.Response.Headers.CacheControl.ToString().ShouldBe("public, max-age=60");
        }

        [Fact]
        public async Task Should_Add_The_Headers_After_The_Response_Was_Cleared()
        {
            // What the exception handler does before writing a 500. Headers set before the clear would be lost from
            // exactly the responses most worth protecting.
            await _middleware.InvokeAsync(_httpContext, context =>
            {
                context.Response.Headers.CacheControl = "public, max-age=60";
                context.Response.Clear();

                return Task.CompletedTask;
            });

            await _responseFeature.StartAsync();

            _httpContext.Response.Headers.XContentTypeOptions.ToString().ShouldBe(SecurityHeadersMiddleware.NoSniff);
            _httpContext.Response.Headers.CacheControl.ToString().ShouldBe(SecurityHeadersMiddleware.NoStore);
        }
    }

    private sealed class StartableResponseFeature : HttpResponseFeature
    {
        private readonly Stack<(Func<object, Task> Callback, object State)> _onStarting = new();

        public override void OnStarting(Func<object, Task> callback, object state)
        {
            _onStarting.Push((callback, state));
        }

        // Last registered runs first, as the servers do.
        public async Task StartAsync()
        {
            while (_onStarting.TryPop(out var registration))
            {
                await registration.Callback(registration.State);
            }
        }
    }
}
