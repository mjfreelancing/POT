using AllOverIt.Fixture.Extensions;
using Shouldly;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Pot.App.Concerns.Time;
using Pot.TestUtils;

namespace Pot.App.Tests.Concerns.Time;

public class PotTimeProviderFixture : PotFixtureBase
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly PotTimeProvider _potTimeProvider;
    private readonly IAppContext _appContext;

    public PotTimeProviderFixture()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        _appContext = Substitute.For<IAppContext>();

        _potTimeProvider = new PotTimeProvider(_appContext)
        {
            TimeProvider = _fakeTimeProvider
        };
    }

    public class Constructor : PotTimeProviderFixture
    {
        [Fact]
        public void Should_Throw_When_AppContext_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() => {
                _ = new PotTimeProvider(null!);
            });

            exception.ParamName.ShouldBe("appContext");
        }
    }

    public class GetUtcDateNow : PotTimeProviderFixture
    {
        [Fact]
        public void Should_Return_Current_Utc_Date()
        {
            var testDate = new DateTime(2026, 1, 28, 14, 30, 45, DateTimeKind.Utc);
            _fakeTimeProvider.SetUtcNow(testDate);

            var result = _potTimeProvider.GetUtcDateNow();

            result.ShouldBe(new DateOnly(2026, 1, 28));
        }
    }

    public class GetUtcDateTimeNow : PotTimeProviderFixture
    {
        [Fact]
        public void Should_Return_Current_Utc_DateTime_With_Utc_Kind()
        {
            var testDate = new DateTime(2026, 1, 28, 14, 30, 45, DateTimeKind.Utc);
            _fakeTimeProvider.SetUtcNow(testDate);

            var result = _potTimeProvider.GetUtcDateTimeNow();

            result.ShouldBe(testDate);
            result.Kind.ShouldBe(DateTimeKind.Utc);
        }
    }

    public class GetLocalDateNow : PotTimeProviderFixture
    {
        [Fact]
        public void Should_Return_Local_Date_Based_On_TimeZone_Offset()
        {
            var utcDate = new DateTime(2026, 1, 28, 22, 30, 0, DateTimeKind.Utc);
            _fakeTimeProvider.SetUtcNow(utcDate);

            _appContext.TimeZoneOffset.Returns(TimeSpan.FromHours(10)); // UTC+10

            var result = _potTimeProvider.GetLocalDateNow();

            // 22:30 UTC + 10 hours = 08:30 next day
            result.ShouldBe(new DateOnly(2026, 1, 29));
        }

        [Fact]
        public void Should_Handle_Negative_TimeZone_Offset()
        {
            var utcDate = new DateTime(2026, 1, 29, 2, 30, 0, DateTimeKind.Utc);
            _fakeTimeProvider.SetUtcNow(utcDate);

            _appContext.TimeZoneOffset.Returns(TimeSpan.FromHours(-5)); // UTC-5

            var result = _potTimeProvider.GetLocalDateNow();

            // 02:30 UTC - 5 hours = 21:30 previous day
            result.ShouldBe(new DateOnly(2026, 1, 28));
        }
    }

    public class GetLocalDateTimeNow : PotTimeProviderFixture
    {
        [Fact]
        public void Should_Return_Local_DateTime_With_Local_Kind()
        {
            var utcDate = new DateTime(2026, 1, 28, 14, 30, 45, DateTimeKind.Utc);
            _fakeTimeProvider.SetUtcNow(utcDate);

            _appContext.TimeZoneOffset.Returns(TimeSpan.FromHours(10)); // UTC+10

            var result = _potTimeProvider.GetLocalDateTimeNow();

            result.ShouldBe(new DateTime(2026, 1, 29, 0, 30, 45));
            result.Kind.ShouldBe(DateTimeKind.Local);
        }

        [Fact]
        public void Should_Handle_Zero_TimeZone_Offset()
        {
            var utcDate = new DateTime(2026, 1, 28, 14, 30, 45, DateTimeKind.Utc);
            _fakeTimeProvider.SetUtcNow(utcDate);

            _appContext.TimeZoneOffset.Returns(TimeSpan.Zero);

            var potTimeProvider = new PotTimeProvider(_appContext)
            {
                TimeProvider = _fakeTimeProvider
            };

            var result = potTimeProvider.GetLocalDateTimeNow();

            result.ShouldBe(new DateTime(2026, 1, 28, 14, 30, 45));
            result.Kind.ShouldBe(DateTimeKind.Local);
        }

        [Fact]
        public void Should_Handle_Large_Positive_TimeZone_Offset()
        {
            var utcDate = new DateTime(2026, 1, 28, 10, 0, 0, DateTimeKind.Utc);
            _fakeTimeProvider.SetUtcNow(utcDate);

            _appContext.TimeZoneOffset.Returns(TimeSpan.FromHours(14)); // UTC+14

            var result = _potTimeProvider.GetLocalDateTimeNow();

            result.ShouldBe(new DateTime(2026, 1, 29, 0, 0, 0));
            result.Kind.ShouldBe(DateTimeKind.Local);
        }
    }

    public class GetLocalTimeZoneOffset : PotTimeProviderFixture
    {
        [Fact]
        public void Should_Return_TimeZone_Offset_From_AppContext()
        {
            var expectedOffset = TimeSpan.FromHours(5.5); // UTC+5:30
            _appContext.TimeZoneOffset.Returns(expectedOffset);

            var result = _potTimeProvider.GetLocalTimeZoneOffset();

            result.ShouldBe(expectedOffset);
        }
    }

    public class DelayAsync : PotTimeProviderFixture
    {
        [Fact]
        public async Task Should_Complete_After_Specified_Delay()
        {
            var delayTask = _potTimeProvider.DelayAsync(TimeSpan.FromSeconds(5), CancellationToken.None);

            delayTask.IsCompleted.ShouldBeFalse();

            _fakeTimeProvider.Advance(TimeSpan.FromSeconds(5));

            await delayTask;

            delayTask.IsCompleted.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Throw_OperationCanceledException_When_Cancelled()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Should.ThrowAsync<OperationCanceledException>(async () => {
                await _potTimeProvider.DelayAsync(TimeSpan.FromSeconds(5), cts.Token);
            });
        }

        [Fact]
        public async Task Should_Complete_Immediately_With_Zero_Delay()
        {
            var delayTask = _potTimeProvider.DelayAsync(TimeSpan.Zero, CancellationToken.None);

            // Should complete immediately without advancing time
            await delayTask;

            delayTask.IsCompleted.ShouldBeTrue();
        }
    }
}
