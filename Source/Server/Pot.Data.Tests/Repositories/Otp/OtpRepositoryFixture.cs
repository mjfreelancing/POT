using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Pot.Data.Entities;
using Pot.Data.Repositories.Otp;
using Pot.Shared;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Repositories.Otp;

public class OtpRepositoryFixture : PotFixtureBase
{
    private const string Username = "otp-user";
    private const string OtherUsername = "other-otp-user";
    private const string ReferenceCode = "123456";

    private sealed class TestContext : IDisposable
    {
        public PotDbContext DbContext { get; }
        public OtpRepository Repository { get; }

        public TestContext(PotDbContext dbContext, OtpRepository repository)
        {
            DbContext = dbContext;
            Repository = repository;
        }

        public Task<int> AddOtpRequestsAsync(params OneTimePasswordEntity[] otpRequests)
        {
            foreach (var otpRequest in otpRequests)
            {
                DbContext.Add(otpRequest);
            }

            return DbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }

    public class GetPendingExpiredAsync : OtpRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_All_Expired_Active_Requests_Regardless_Of_Reason()
        {
            using var context = CreateTestContext();
            var now = CreateNow();

            var expiredSignup = CreateOtp(OtpReason.Signup, OtpStatus.Active, now.AddMinutes(-30), now);
            var expiredPasswordReset = CreateOtp(OtpReason.PasswordReset, OtpStatus.Active, now.AddMinutes(-30), now.AddMinutes(-1));
            var notExpired = CreateOtp(OtpReason.Signup, OtpStatus.Active, now.AddMinutes(-30), now.AddMinutes(10));
            var expiredUsed = CreateOtp(OtpReason.Signup, OtpStatus.Used, now.AddMinutes(-30), now.AddMinutes(-5));

            await context.AddOtpRequestsAsync(expiredSignup, expiredPasswordReset, notExpired, expiredUsed);

            var result = await context.Repository.GetPendingExpiredAsync(reason: null, now, Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(2);
            result.ShouldHaveValues(otpRequest => otpRequest.RowId, new[] { expiredSignup.RowId, expiredPasswordReset.RowId });
        }

        [Fact]
        public async Task Should_Filter_By_Reason_When_Provided()
        {
            using var context = CreateTestContext();
            var now = CreateNow();

            var expiredSignup = CreateOtp(OtpReason.Signup, OtpStatus.Active, now.AddMinutes(-30), now);
            var expiredPasswordReset = CreateOtp(OtpReason.PasswordReset, OtpStatus.Active, now.AddMinutes(-30), now);

            await context.AddOtpRequestsAsync(expiredSignup, expiredPasswordReset);

            var result = await context.Repository.GetPendingExpiredAsync(OtpReason.Signup, now, Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(expiredSignup.RowId);
        }

        [Fact]
        public async Task Should_Return_Empty_List_When_No_Requests_Have_Expired()
        {
            using var context = CreateTestContext();
            var now = CreateNow();

            var notExpired = CreateOtp(OtpReason.Signup, OtpStatus.Active, now.AddMinutes(-1), now.AddMinutes(10));
            await context.AddOtpRequestsAsync(notExpired);

            var result = await context.Repository.GetPendingExpiredAsync(reason: null, now, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeEmpty();
        }
    }

    public class CountFailedRequestsForUsernameAsync : OtpRepositoryFixture
    {
        [Fact]
        public async Task Should_Count_Failed_Requests_For_The_Username_After_The_Date()
        {
            using var context = CreateTestContext();
            var now = CreateNow();

            var failedRecent = CreateOtp(OtpReason.Signup, OtpStatus.Failed, now.AddMinutes(-10), now.AddMinutes(5), Username);
            var failedOld = CreateOtp(OtpReason.Signup, OtpStatus.Failed, now.AddHours(-2), now.AddHours(-1), Username);
            var failedOtherUser = CreateOtp(OtpReason.Signup, OtpStatus.Failed, now.AddMinutes(-10), now.AddMinutes(5), OtherUsername);
            var activeRecent = CreateOtp(OtpReason.Signup, OtpStatus.Active, now.AddMinutes(-10), now.AddMinutes(5), Username);

            await context.AddOtpRequestsAsync(failedRecent, failedOld, failedOtherUser, activeRecent);

            var result = await context.Repository.CountFailedRequestsForUsernameAsync(
                reason: null,
                Username,
                afterDateUtc: now.AddMinutes(-30),
                Xunit.TestContext.Current.CancellationToken);

            result.ShouldBe(1);
        }

        [Fact]
        public async Task Should_Filter_By_Reason_When_Provided()
        {
            using var context = CreateTestContext();
            var now = CreateNow();

            var failedSignup = CreateOtp(OtpReason.Signup, OtpStatus.Failed, now.AddMinutes(-10), now.AddMinutes(5), Username);
            var failedPasswordReset = CreateOtp(OtpReason.PasswordReset, OtpStatus.Failed, now.AddMinutes(-10), now.AddMinutes(5), Username);

            await context.AddOtpRequestsAsync(failedSignup, failedPasswordReset);

            var result = await context.Repository.CountFailedRequestsForUsernameAsync(
                OtpReason.PasswordReset,
                Username,
                afterDateUtc: now.AddMinutes(-30),
                Xunit.TestContext.Current.CancellationToken);

            result.ShouldBe(1);
        }

        [Fact]
        public async Task Should_Return_Zero_When_The_Username_Is_Null()
        {
            using var context = CreateTestContext();
            var now = CreateNow();

            var failedSignup = CreateOtp(OtpReason.Signup, OtpStatus.Failed, now.AddMinutes(-10), now.AddMinutes(5), Username);
            await context.AddOtpRequestsAsync(failedSignup);

            var result = await context.Repository.CountFailedRequestsForUsernameAsync(
                reason: null,
                username: null,
                afterDateUtc: now.AddMinutes(-30),
                Xunit.TestContext.Current.CancellationToken);

            result.ShouldBe(0);
        }
    }

    public class GetActiveRequestsForUsernameAsync : OtpRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_Active_Requests_For_The_Username()
        {
            using var context = CreateTestContext();
            var now = CreateNow();

            var active = CreateOtp(OtpReason.Signup, OtpStatus.Active, now, now.AddMinutes(10), Username);
            var used = CreateOtp(OtpReason.Signup, OtpStatus.Used, now, now.AddMinutes(10), Username);
            var otherUserActive = CreateOtp(OtpReason.Signup, OtpStatus.Active, now, now.AddMinutes(10), OtherUsername);

            await context.AddOtpRequestsAsync(active, used, otherUserActive);

            var result = await context.Repository.GetActiveRequestsForUsernameAsync(reason: null, Username, Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(active.RowId);
        }

        [Fact]
        public async Task Should_Filter_By_Reason_When_Provided()
        {
            using var context = CreateTestContext();
            var now = CreateNow();

            var activeSignup = CreateOtp(OtpReason.Signup, OtpStatus.Active, now, now.AddMinutes(10), Username);
            var activePasswordReset = CreateOtp(OtpReason.PasswordReset, OtpStatus.Active, now, now.AddMinutes(10), Username);

            await context.AddOtpRequestsAsync(activeSignup, activePasswordReset);

            var result = await context.Repository.GetActiveRequestsForUsernameAsync(OtpReason.Signup, Username, Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(activeSignup.RowId);
        }

        [Fact]
        public async Task Should_Return_Empty_List_When_There_Are_No_Active_Requests()
        {
            using var context = CreateTestContext();
            var now = CreateNow();

            var used = CreateOtp(OtpReason.Signup, OtpStatus.Used, now, now.AddMinutes(10), Username);
            await context.AddOtpRequestsAsync(used);

            var result = await context.Repository.GetActiveRequestsForUsernameAsync(reason: null, Username, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeEmpty();
        }
    }

    public class GetRequestsForUsernameAndRefCodeAsync : OtpRepositoryFixture
    {
        [Fact]
        public async Task Should_Return_The_Matching_Request()
        {
            using var context = CreateTestContext();
            var now = CreateNow();

            var matching = CreateOtp(OtpReason.Signup, OtpStatus.Active, now, now.AddMinutes(10), Username, ReferenceCode);
            var differentRefCode = CreateOtp(OtpReason.Signup, OtpStatus.Active, now, now.AddMinutes(10), Username, "654321");

            await context.AddOtpRequestsAsync(matching, differentRefCode);

            var result = await context.Repository.GetRequestsForUsernameAndRefCodeAsync(OtpReason.Signup, Username, ReferenceCode, Xunit.TestContext.Current.CancellationToken);

            result.Count.ShouldBe(1);
            result[0].RowId.ShouldBe(matching.RowId);
        }

        [Fact]
        public async Task Should_Return_Empty_List_When_Nothing_Matches()
        {
            using var context = CreateTestContext();
            var now = CreateNow();

            var passwordReset = CreateOtp(OtpReason.PasswordReset, OtpStatus.Active, now, now.AddMinutes(10), Username, ReferenceCode);
            await context.AddOtpRequestsAsync(passwordReset);

            var result = await context.Repository.GetRequestsForUsernameAndRefCodeAsync(OtpReason.Signup, Username, ReferenceCode, Xunit.TestContext.Current.CancellationToken);

            result.ShouldBeEmpty();
        }
    }

    private static DateTime CreateNow()
    {
        return new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
    }

    private static OneTimePasswordEntity CreateOtp(OtpReason reason, OtpStatus status, DateTime createdUtc, DateTime expiryUtc,
        string username = Username, string referenceCode = ReferenceCode)
    {
        return new OneTimePasswordEntity
        {
            RowId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid().ToString("N"),
            Username = username,
            Email = $"{username}@example.com",
            Reason = reason,
            RefCode = referenceCode,
            OtpCode = "654321",
            Status = status,
            CreatedUtc = createdUtc,
            ExpiryUtc = expiryUtc
        };
    }

    private static TestContext CreateTestContext()
    {
        var dbOptions = new DbContextOptionsBuilder<PotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var site = EntityFactory.CreateSite();
        var user = EntityFactory.CreateUser(site);

        var currentUserContext = Substitute.For<ICurrentUserContext>();
        currentUserContext.UserRowId.Returns(user.RowId);

        var dbContext = new PotDbContext(dbOptions, currentUserContext);

        dbContext.Add(site);
        dbContext.Add(user);
        dbContext.SaveChanges();

        var repository = new OtpRepository(dbContext);

        return new TestContext(dbContext, repository);
    }
}
