using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Pot.App.Concerns.Time;
using Pot.App.Features.Notifications.BudgetReminder;
using Pot.App.Features.Users.GetAll;
using Pot.App.Features.Users.GetAll.Models;
using Pot.AspNetCore.Concerns.Health;
using Pot.AspNetCore.Features.Workers;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.AspNetCore.Tests.Features.Workers;

public class BudgetReminderEmailWorkerFixture : PotFixtureBase
{
    private const string DatabaseHealthCheckName = "database";

    private static readonly TimeSpan HealthCheckRequestTimeout = TimeSpan.FromSeconds(5);

    private static readonly TimeSpan UserProcessingTimeout = TimeSpan.FromSeconds(5);

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

            var pollerOptions = await healthCheckRequested.Task.WaitAsync(HealthCheckRequestTimeout, TestContext.Current.CancellationToken);

            pollerOptions.Name.ShouldBe(DatabaseHealthCheckName);
            _ = scopeFactoryFake.DidNotReceive().CreateScope();

            await hostedService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task Should_Attempt_Remaining_Users_When_A_User_Fails()
        {
            var users = CreateUsers(3);
            var failingUser = users[1];

            var attemptedUserRowIds = new List<Guid>();
            var allUsersAttempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var currentUserContext = new TestCurrentUserContext();

            var getAllUsersServiceFake = Substitute.For<IGetAllUsersService>();

            getAllUsersServiceFake
                .GetAllEnabledAdminsAsync(Arg.Any<CancellationToken>())
                .Returns(users);

            var budgetReminderServiceFake = Substitute.For<IBudgetReminderService>();

            budgetReminderServiceFake
                .SendRemindersAsync(Arg.Any<CancellationToken>())
                .Returns(_ =>
                {
                    var userRowId = currentUserContext.UserRowId;

                    attemptedUserRowIds.Add(userRowId);

                    if (attemptedUserRowIds.Count == users.Count)
                    {
                        allUsersAttempted.TrySetResult();
                    }

                    if (userRowId == failingUser.RowId)
                    {
                        throw new InvalidOperationException("Simulated failure for a single user.");
                    }

                    return Task.CompletedTask;
                });

            using var serviceProvider = CreateServiceProvider(getAllUsersServiceFake, currentUserContext, budgetReminderServiceFake);

            var scopeFactoryFake = Substitute.For<IServiceScopeFactory>();
            scopeFactoryFake.CreateScope().Returns(_ => CreateScope(serviceProvider));

            var timeProviderFake = Substitute.For<ITimeProvider>();

            timeProviderFake.GetUtcDateTimeNow().Returns(new DateTime(2026, 9, 23, 5, 10, 0, DateTimeKind.Utc));

            timeProviderFake
                .DelayAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.Delay(Timeout.InfiniteTimeSpan, callInfo.Arg<CancellationToken>()));

            var serviceHealthPollerFake = Substitute.For<IServiceHealthPoller>();

            serviceHealthPollerFake
                .WaitForHealthyAsync(Arg.Any<ServiceHealthPollerOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            var worker = CreateWorker(
                Substitute.For<IHostApplicationLifetime>(),
                timeProviderFake,
                scopeFactoryFake,
                serviceHealthPollerFake);

            var hostedService = (IHostedService)worker;

            await hostedService.StartAsync(CancellationToken.None);

            await allUsersAttempted.Task.WaitAsync(UserProcessingTimeout, TestContext.Current.CancellationToken);

            attemptedUserRowIds.ShouldBe([users[0].RowId, users[1].RowId, users[2].RowId]);

            await hostedService.StopAsync(CancellationToken.None);
        }
    }

    private static List<Output> CreateUsers(int count)
    {
        return [.. Enumerable
            .Range(1, count)
            .Select(index => new Output
            {
                RowId = Guid.NewGuid(),
                Etag = index,
                Username = $"user{index}",
                DisplayName = $"User {index}",
                Email = $"user{index}@example.com",
                Status = UserStatus.Enabled.Name,
                Roles = [Role.Admin.Name]
            })];
    }

    private static ServiceProvider CreateServiceProvider(IGetAllUsersService getAllUsersService,
        ICurrentUserContext currentUserContext, IBudgetReminderService budgetReminderService)
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddSingleton<IGetAllUsersService>(getAllUsersService);
        services.AddSingleton<ICurrentUserContext>(currentUserContext);
        services.AddSingleton<IBudgetReminderService>(budgetReminderService);

        return services.BuildServiceProvider();
    }

    private static IServiceScope CreateScope(IServiceProvider serviceProvider)
    {
        var scope = Substitute.For<IServiceScope>();

        scope.ServiceProvider.Returns(serviceProvider);

        return scope;
    }

    private static BudgetReminderEmailWorker CreateWorker(IHostApplicationLifetime applicationLifetime, ITimeProvider timeProvider,
        IServiceScopeFactory scopeFactory, IServiceHealthPoller serviceHealthPoller)
    {
        return new BudgetReminderEmailWorker(applicationLifetime, timeProvider, scopeFactory, serviceHealthPoller);
    }

    private sealed class TestCurrentUserContext : ICurrentUserContext
    {
        private Guid? _userRowId;

        public Guid UserRowId => _userRowId ?? throw new InvalidOperationException("The user identifier has not been set.");

        public void SetUserRowId(Guid userRowId)
        {
            _userRowId = userRowId;
        }
    }
}
