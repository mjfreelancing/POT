using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Pot.App.Concerns.Time;
using Pot.AspNetCore.Concerns.Health;
using Pot.AspNetCore.Features.Workers;
using Pot.TestUtils;
using Shouldly;

namespace Pot.AspNetCore.Tests.Features.Workers;

public class BudgetReminderEmailWorkerFixture : PotFixtureBase
{
    private const string DatabaseHealthCheckName = "database";

    private static readonly TimeSpan HealthCheckRequestTimeout = TimeSpan.FromSeconds(5);

    // The IHostApplicationLifetime guard lives on the AllOverIt BackgroundWorker base type, so it is not re-tested here.
    public class Constructor : BudgetReminderEmailWorkerFixture
    {
        private readonly IHostApplicationLifetime _applicationLifetimeFake;

        public Constructor()
        {
            _applicationLifetimeFake = Substitute.For<IHostApplicationLifetime>();
        }

        [Fact]
        public void Should_Throw_When_TimeProvider_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = CreateWorker(_applicationLifetimeFake, null!, Substitute.For<IServiceScopeFactory>(), Substitute.For<IServiceHealthPoller>());
            });

            exception.ParamName.ShouldBe("timeProvider");
        }

        [Fact]
        public void Should_Throw_When_ScopeFactory_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = CreateWorker(_applicationLifetimeFake, Substitute.For<ITimeProvider>(), null!, Substitute.For<IServiceHealthPoller>());
            });

            exception.ParamName.ShouldBe("scopeFactory");
        }

        [Fact]
        public void Should_Throw_When_ServiceHealthPoller_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = CreateWorker(_applicationLifetimeFake, Substitute.For<ITimeProvider>(), Substitute.For<IServiceScopeFactory>(), null!);
            });

            exception.ParamName.ShouldBe("serviceHealthPoller");
        }
    }

    public class ExecuteAsync : BudgetReminderEmailWorkerFixture
    {
        [Fact]
        public async Task Should_Wait_For_Database_Health_Before_Starting_Its_First_Cycle()
        {
            var healthCheckRequested = new TaskCompletionSource<ServiceHealthPollerOptions>(TaskCreationOptions.RunContinuationsAsynchronously);

            var serviceHealthPollerFake = Substitute.For<IServiceHealthPoller>();

            serviceHealthPollerFake
                .When(poller => poller.WaitForHealthyAsync(Arg.Any<ServiceHealthPollerOptions>(), Arg.Any<CancellationToken>()))
                .Do(callInfo => healthCheckRequested.TrySetResult(callInfo.Arg<ServiceHealthPollerOptions>()));

            serviceHealthPollerFake
                .WaitForHealthyAsync(Arg.Any<ServiceHealthPollerOptions>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.Delay(Timeout.InfiniteTimeSpan, callInfo.Arg<CancellationToken>()));

            var scopeFactoryFake = Substitute.For<IServiceScopeFactory>();

            var worker = CreateWorker(
                Substitute.For<IHostApplicationLifetime>(),
                Substitute.For<ITimeProvider>(),
                scopeFactoryFake,
                serviceHealthPollerFake);

            var hostedService = (IHostedService)worker;

            await hostedService.StartAsync(CancellationToken.None);

            var pollerOptions = await healthCheckRequested.Task.WaitAsync(HealthCheckRequestTimeout);

            pollerOptions.Name.ShouldBe(DatabaseHealthCheckName);
            _ = scopeFactoryFake.DidNotReceive().CreateScope();

            await hostedService.StopAsync(CancellationToken.None);
        }
    }

    private static BudgetReminderEmailWorker CreateWorker(IHostApplicationLifetime applicationLifetime, ITimeProvider timeProvider,
        IServiceScopeFactory scopeFactory, IServiceHealthPoller serviceHealthPoller)
    {
        return new BudgetReminderEmailWorker(applicationLifetime, timeProvider, scopeFactory, serviceHealthPoller);
    }
}
