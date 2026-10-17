using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Pot.AspNetCore.Integration.Tests.Host;
using Pot.Data;
using Shouldly;
using System.Data.Common;
using System.Net;

namespace Pot.AspNetCore.Integration.Tests.Features.Auth;

/// <summary>
/// Pins the cost of validating an access token: the user's security state and the session named by the token are
/// resolved by one statement, so authentication does not add a second round trip per request.
/// </summary>
public class SessionValidationRoundTripFixture : IntegrationAuthFixtureBase
{
    private CountingApiWebApplicationFactory? _countingFactory;

    protected override ApiWebApplicationFactory CreateApiWebApplicationFactory(string dbHost, int dbPort)
    {
        _countingFactory = new CountingApiWebApplicationFactory(dbHost, dbPort);

        return _countingFactory;
    }

    [Fact]
    public async Task Should_Resolve_The_Session_State_Without_A_Second_Query()
    {
        var user = await CreateEnabledUserAsync("roundtrip", "Round Trip User");
        var loginResult = await LoginAsync(user);

        var countingFactory = _countingFactory.ShouldNotBeNull();

        countingFactory.Interceptor.Reset();

        var currentUserStatus = await GetCurrentUserAsync(loginResult.AccessToken!);

        currentUserStatus.ShouldBe(HttpStatusCode.OK);

        // Filtered to the statement that joins the sessions, because the endpoint itself issues its own queries.
        var sessionStatements = countingFactory.Interceptor.CommandTexts
            .Count(commandText => commandText.Contains("AuthSession", StringComparison.OrdinalIgnoreCase));

        sessionStatements.ShouldBe(1);
    }

    /// <summary>
    /// Records the text of every command the context executes, so a test can assert how many statements
    /// authentication costs.
    /// </summary>
    private sealed class CommandCountingInterceptor : DbCommandInterceptor
    {
        private readonly List<string> _commandTexts = new();
        private readonly Lock _gate = new();

        public IReadOnlyList<string> CommandTexts
        {
            get
            {
                lock (_gate)
                {
                    return _commandTexts.ToArray();
                }
            }
        }

        public void Reset()
        {
            lock (_gate)
            {
                _commandTexts.Clear();
            }
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            Record(command);

            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Record(command);

            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        private void Record(DbCommand command)
        {
            lock (_gate)
            {
                _commandTexts.Add(command.CommandText);
            }
        }
    }

    /// <summary>
    /// Production-configured host that also records the commands the application's context executes.
    /// </summary>
    private sealed class CountingApiWebApplicationFactory : ApiWebApplicationFactory
    {
        public CountingApiWebApplicationFactory(string dbHost, int dbPort)
            : base(dbHost, dbPort)
        {
        }

        public CommandCountingInterceptor Interceptor { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.UseEnvironment("Production");

            builder.ConfigureServices(services =>
            {
                services.AddSingleton(Interceptor);

                services.AddDbContext<PotDbContext>((provider, options) =>
                    options.AddInterceptors(provider.GetRequiredService<CommandCountingInterceptor>()));
            });
        }
    }
}
