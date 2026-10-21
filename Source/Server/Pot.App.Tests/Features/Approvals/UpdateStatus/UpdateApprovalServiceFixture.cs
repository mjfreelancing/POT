using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Features.Approvals.UpdateStatus;
using Pot.App.Features.Approvals.UpdateStatus.EntityChecks;
using Pot.App.Features.Approvals.UpdateStatus.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.EmailSender;
using Pot.RazorComponents.Models;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Approvals.UpdateStatus;

public class UpdateApprovalServiceFixture : PotFixtureBase
{
    public class UpdateUserApprovalAsync : UpdateApprovalServiceFixture
    {
        private readonly IPersistableUserRepository _userRepositoryFake = Substitute.For<IPersistableUserRepository>();
        private readonly IPreUpdateChecker _preUpdateCheckerFake = Substitute.For<IPreUpdateChecker>();
        private readonly ISendEmailChannelWriter _emailWriterFake = Substitute.For<ISendEmailChannelWriter>();

        [Fact]
        public async Task Should_Address_The_Accepted_Email_To_The_Users_Display_Name()
        {
            var user = ArrangeUser();

            await CreateService().UpdateUserApprovalAsync(CreateInput(user, ApprovalStatus.Approved), CancellationToken.None);

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.ApprovalAccepted,
                Arg.Is<EmailApprovalStatusInfo>(info => info.Username == user.Username &&
                    info.DisplayName == user.DisplayName &&
                    info.RecipientName == user.DisplayName),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Address_The_Rejected_Email_To_The_Users_Display_Name()
        {
            var user = ArrangeUser();

            await CreateService().UpdateUserApprovalAsync(CreateInput(user, ApprovalStatus.Rejected), CancellationToken.None);

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.ApprovalRejected,
                Arg.Is<EmailApprovalStatusInfo>(info => info.Username == user.Username &&
                    info.DisplayName == user.DisplayName &&
                    info.RecipientName == user.DisplayName),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Fail_And_Not_Write_Or_Notify_When_The_User_Does_Not_Exist()
        {
            _userRepositoryFake
                .GetByIdForApprovalAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((UserEntity?)null);

            var result = await CreateService()
                .UpdateUserApprovalAsync(CreateInput(EntityFactory.CreateUser(EntityFactory.CreateSite()), ApprovalStatus.Approved),
                    CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();

            await _userRepositoryFake.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());

            await _emailWriterFake.DidNotReceive().SubmitAsync(
                Arg.Any<EmailType>(), Arg.Any<EmailConfigBase>(), Arg.Any<CancellationToken>());
        }

        private UserEntity ArrangeUser()
        {
            var site = EntityFactory.CreateSite(name: "Approval Site");
            var user = EntityFactory.CreateUser(site, "jsmith", "jane@example.com", "Jane Smith");

            _userRepositoryFake
                .GetByIdForApprovalAsync(user.RowId, Arg.Any<CancellationToken>())
                .Returns(user);

            return user;
        }

        private static Input CreateInput(UserEntity user, ApprovalStatus status)
        {
            return new Input
            {
                RowId = user.RowId,
                Etag = user.Etag,
                Status = status
            };
        }

        private UpdateApprovalService CreateService()
        {
            return new UpdateApprovalService(
                _userRepositoryFake,
                _emailWriterFake,
                _preUpdateCheckerFake,
                Substitute.For<ILogger<UpdateApprovalService>>());
        }
    }
}
