using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Pot.App.Concerns.Time;
using Pot.App.Concerns.Time.Extensions;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Concerns.Time.Extensions;

public class TimeProviderExtensionsFixture : PotFixtureBase
{
    public class WaitUntilUtcAsync : TimeProviderExtensionsFixture
    {
        private readonly FakeTimeProvider _fakeTimeProvider;
        private readonly ITimeProvider _timeProvider;

        public WaitUntilUtcAsync()
        {
            _fakeTimeProvider = new FakeTimeProvider();
            _timeProvider = Substitute.For<ITimeProvider>();

            // Mock DelayAsync to use Task.Delay with FakeTimeProvider
            _timeProvider.DelayAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    var delay = callInfo.ArgAt<TimeSpan>(0);
                    var token = callInfo.ArgAt<CancellationToken>(1);

                    return Task.Delay(delay, _fakeTimeProvider, token);
                });
        }

        [Fact]
        public async Task Should_Return_Immediately_When_Target_Time_Is_In_Past()
        {
            var currentUtc = new DateTime(2026, 1, 28, 12, 0, 0, DateTimeKind.Utc);
            var targetUtc = new DateTime(2026, 1, 28, 11, 0, 0, DateTimeKind.Utc);

            _fakeTimeProvider.SetUtcNow(currentUtc);
            _timeProvider.GetUtcDateTimeNow().Returns(_ => _fakeTimeProvider.GetUtcNow().DateTime);

            await _timeProvider.WaitUntilUtcAsync(targetUtc, CancellationToken.None);

            _timeProvider.Received(1).GetUtcDateTimeNow();
        }

        [Fact]
        public async Task Should_Return_Immediately_When_Target_Time_Equals_Current_Time()
        {
            var currentUtc = new DateTime(2026, 1, 28, 12, 0, 0, DateTimeKind.Utc);
            var targetUtc = new DateTime(2026, 1, 28, 12, 0, 0, DateTimeKind.Utc);

            _fakeTimeProvider.SetUtcNow(currentUtc);
            _timeProvider.GetUtcDateTimeNow().Returns(_ => _fakeTimeProvider.GetUtcNow().DateTime);

            await _timeProvider.WaitUntilUtcAsync(targetUtc, CancellationToken.None);

            _timeProvider.Received(1).GetUtcDateTimeNow();
        }

        [Fact]
        public async Task Should_Wait_Until_Target_Time_Is_Reached()
        {
            var currentUtc = new DateTime(2026, 1, 28, 12, 0, 0, DateTimeKind.Utc);
            var targetUtc = currentUtc.AddSeconds(5);

            _fakeTimeProvider.SetUtcNow(currentUtc);
            _timeProvider.GetUtcDateTimeNow().Returns(_ => _fakeTimeProvider.GetUtcNow().DateTime);

            var waitTask = _timeProvider.WaitUntilUtcAsync(targetUtc, CancellationToken.None);

            // Time hasn't advanced yet, so the task should not be complete
            waitTask.IsCompleted.ShouldBeFalse();

            // Advance time to the target
            _fakeTimeProvider.Advance(TimeSpan.FromSeconds(5));

            // Wait for the task to complete
            await waitTask;

            _timeProvider.Received(2).GetUtcDateTimeNow();
        }

        [Fact]
        public async Task Should_Throw_OperationCanceledException_When_Cancellation_Requested()
        {
            var currentUtc = new DateTime(2026, 1, 28, 12, 0, 0, DateTimeKind.Utc);
            var targetUtc = new DateTime(2026, 1, 28, 13, 0, 0, DateTimeKind.Utc);

            _fakeTimeProvider.SetUtcNow(currentUtc);
            _timeProvider.GetUtcDateTimeNow().Returns(_ => _fakeTimeProvider.GetUtcNow().DateTime);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Should.ThrowAsync<OperationCanceledException>(async () =>
            {
                await _timeProvider.WaitUntilUtcAsync(targetUtc, cts.Token);
            });
        }

        [Fact]
        public async Task Should_Throw_OperationCanceledException_When_Cancelled_During_Wait()
        {
            var currentUtc = new DateTime(2026, 1, 28, 12, 0, 0, DateTimeKind.Utc);
            var targetUtc = currentUtc.AddSeconds(10);

            _fakeTimeProvider.SetUtcNow(currentUtc);
            _timeProvider.GetUtcDateTimeNow().Returns(_ => _fakeTimeProvider.GetUtcNow().DateTime);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Should.ThrowAsync<OperationCanceledException>(async () =>
            {
                await _timeProvider.WaitUntilUtcAsync(targetUtc, cts.Token);
            });
        }

        [Fact]
        public async Task Should_Handle_Time_Drift_By_Checking_Multiple_Times()
        {
            var currentUtc = new DateTime(2026, 1, 28, 12, 0, 0, DateTimeKind.Utc);
            var targetUtc = currentUtc.AddSeconds(10);

            _fakeTimeProvider.SetUtcNow(currentUtc);

            var callCount = 0;
            _timeProvider.GetUtcDateTimeNow().Returns(_ =>
            {
                callCount++;
                return _fakeTimeProvider.GetUtcNow().DateTime;
            });

            // Override DelayAsync for this test to advance only partially on first call
            var firstCall = true;
            _timeProvider.DelayAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    var delay = callInfo.ArgAt<TimeSpan>(0);
                    var token = callInfo.ArgAt<CancellationToken>(1);

                    if (firstCall)
                    {
                        // First delay only advances halfway (simulating drift)
                        _fakeTimeProvider.Advance(delay / 2);
                        firstCall = false;
                    }
                    else
                    {
                        // Subsequent delays advance fully
                        _fakeTimeProvider.Advance(delay);
                    }

                    return token.IsCancellationRequested ? Task.FromCanceled(token) : Task.CompletedTask;
                });

            await _timeProvider.WaitUntilUtcAsync(targetUtc, CancellationToken.None);

            callCount.ShouldBeGreaterThan(1);
        }

        [Fact]
        public async Task Should_Work_With_Short_Delay()
        {
            var currentUtc = new DateTime(2026, 1, 28, 12, 0, 0, DateTimeKind.Utc);
            var targetUtc = currentUtc.AddMilliseconds(100);

            _fakeTimeProvider.SetUtcNow(currentUtc);
            _timeProvider.GetUtcDateTimeNow().Returns(_ => _fakeTimeProvider.GetUtcNow().DateTime);

            var waitTask = _timeProvider.WaitUntilUtcAsync(targetUtc, CancellationToken.None);

            waitTask.IsCompleted.ShouldBeFalse();

            _fakeTimeProvider.Advance(TimeSpan.FromMilliseconds(100));

            await waitTask;

            _timeProvider.Received(2).GetUtcDateTimeNow();
        }

        [Fact]
        public async Task Should_Handle_System_Time_Change_Backwards()
        {
            var currentUtc = new DateTime(2026, 1, 28, 12, 0, 0, DateTimeKind.Utc);
            var targetUtc = currentUtc.AddSeconds(5);

            // _fakeTimeProvider cannot go backwards in time.
            // Simulate system time going backwards by returning specific values.
            _timeProvider.GetUtcDateTimeNow().Returns(
                currentUtc,
                currentUtc.AddSeconds(-2), // Time goes backwards
                currentUtc.AddSeconds(1),
                targetUtc
            );

            // Mock DelayAsync to return immediately for this test
            _timeProvider.DelayAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            await _timeProvider.WaitUntilUtcAsync(targetUtc, CancellationToken.None);

            _timeProvider.Received(4).GetUtcDateTimeNow();
        }
    }
}
