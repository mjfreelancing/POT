using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.AspNetCore.Concerns.Health;
using Pot.TestUtils;
using Shouldly;

namespace Pot.AspNetCore.Tests.Concerns.Health;

public class ServiceHealthPollerFixture : PotFixtureBase
{
    private const string DatabaseHealthCheckName = "database";

    public class Constructor : ServiceHealthPollerFixture
    {
        [Fact]
        public void Should_Throw_When_HealthCheckService_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new ServiceHealthPoller(null!, Substitute.For<ILogger<ServiceHealthPoller>>());
            });

            exception.ParamName.ShouldBe("healthCheckService");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new ServiceHealthPoller(Substitute.For<HealthCheckService>(), null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class WaitForHealthyAsync : ServiceHealthPollerFixture
    {
        private readonly HealthCheckService _healthCheckServiceFake;

        public WaitForHealthyAsync()
        {
            _healthCheckServiceFake = Substitute.For<HealthCheckService>();
        }

        [Fact]
        public async Task Should_Return_Without_Waiting_When_Health_Check_Is_Healthy()
        {
            _healthCheckServiceFake
                .CheckHealthAsync(Arg.Any<Func<HealthCheckRegistration, bool>>(), Arg.Any<CancellationToken>())
                .Returns(CreateHealthReport(HealthStatus.Healthy));

            var serviceHealthPoller = CreateServiceHealthPoller(_healthCheckServiceFake);

            var waitTask = serviceHealthPoller.WaitForHealthyAsync(CreatePollerOptions(), CancellationToken.None);

            (await Task.WhenAny(waitTask, Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken))).ShouldBe(waitTask);

            _ = _healthCheckServiceFake
                .Received(1)
                .CheckHealthAsync(Arg.Any<Func<HealthCheckRegistration, bool>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Poll_Until_Healthy_When_Health_Check_Is_Not_Ready_At_First()
        {
            _healthCheckServiceFake
                .CheckHealthAsync(Arg.Any<Func<HealthCheckRegistration, bool>>(), Arg.Any<CancellationToken>())
                .Returns(
                    CreateHealthReport(HealthStatus.Unhealthy),
                    CreateHealthReport(HealthStatus.Degraded),
                    CreateHealthReport(HealthStatus.Healthy));

            var serviceHealthPoller = CreateServiceHealthPoller(_healthCheckServiceFake);

            await serviceHealthPoller.WaitForHealthyAsync(CreatePollerOptions(TimeSpan.FromMilliseconds(20)), CancellationToken.None);

            _ = _healthCheckServiceFake
                .Received(3)
                .CheckHealthAsync(Arg.Any<Func<HealthCheckRegistration, bool>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Poll_Only_The_Requested_Health_Check()
        {
            Func<HealthCheckRegistration, bool>? registrationFilter = null;

            _healthCheckServiceFake
                .CheckHealthAsync(Arg.Do<Func<HealthCheckRegistration, bool>>(filter => registrationFilter = filter), Arg.Any<CancellationToken>())
                .Returns(CreateHealthReport(HealthStatus.Healthy));

            var serviceHealthPoller = CreateServiceHealthPoller(_healthCheckServiceFake);

            await serviceHealthPoller.WaitForHealthyAsync(CreatePollerOptions(), CancellationToken.None);

            registrationFilter.ShouldNotBeNull();
            registrationFilter(CreateHealthCheckRegistration(DatabaseHealthCheckName)).ShouldBeTrue();
            registrationFilter(CreateHealthCheckRegistration("some-other-check")).ShouldBeFalse();
        }

        [Fact]
        public async Task Should_Not_Check_Health_When_Cancellation_Is_Already_Requested()
        {
            using var cancellationSource = new CancellationTokenSource();
            await cancellationSource.CancelAsync();

            var serviceHealthPoller = CreateServiceHealthPoller(_healthCheckServiceFake);

            await serviceHealthPoller.WaitForHealthyAsync(CreatePollerOptions(), cancellationSource.Token);

            _ = _healthCheckServiceFake
                .DidNotReceive()
                .CheckHealthAsync(Arg.Any<Func<HealthCheckRegistration, bool>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Stop_Waiting_When_Cancelled_While_Waiting_Between_Polls()
        {
            using var cancellationSource = new CancellationTokenSource();

            _healthCheckServiceFake
                .CheckHealthAsync(Arg.Any<Func<HealthCheckRegistration, bool>>(), Arg.Any<CancellationToken>())
                .Returns(_ =>
                {
                    cancellationSource.Cancel();

                    return CreateHealthReport(HealthStatus.Unhealthy);
                });

            var serviceHealthPoller = CreateServiceHealthPoller(_healthCheckServiceFake);

            await Should.ThrowAsync<OperationCanceledException>(() =>
                serviceHealthPoller.WaitForHealthyAsync(CreatePollerOptions(), cancellationSource.Token));

            _ = _healthCheckServiceFake.Received(1).CheckHealthAsync(Arg.Any<Func<HealthCheckRegistration, bool>>(), Arg.Any<CancellationToken>());
        }
    }

    private static ServiceHealthPollerOptions CreatePollerOptions(TimeSpan? pollingInterval = null)
    {
        return new ServiceHealthPollerOptions
        {
            Name = DatabaseHealthCheckName,
            PollingInterval = pollingInterval ?? TimeSpan.FromSeconds(30)
        };
    }

    private static HealthReport CreateHealthReport(HealthStatus status)
    {
        var entries = new Dictionary<string, HealthReportEntry>();

        if (status != HealthStatus.Healthy)
        {
            entries[DatabaseHealthCheckName] = new HealthReportEntry(status, description: null, duration: TimeSpan.Zero, exception: null, data: null);
        }

        return new HealthReport(entries, totalDuration: TimeSpan.Zero);
    }

    private static HealthCheckRegistration CreateHealthCheckRegistration(string name)
    {
        return new HealthCheckRegistration(name, Substitute.For<IHealthCheck>(), failureStatus: null, tags: null);
    }

    private static ServiceHealthPoller CreateServiceHealthPoller(HealthCheckService healthCheckService)
    {
        return new ServiceHealthPoller(healthCheckService, Substitute.For<ILogger<ServiceHealthPoller>>());
    }
}
