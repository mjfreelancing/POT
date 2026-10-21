using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Auth;
using Pot.App.Features.Users.Invite;
using Pot.App.Features.Users.Invite.EntityChecks;
using Pot.App.Features.Users.Invite.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Roles;
using Pot.Data.Repositories.Sites;
using Pot.Data.Repositories.Users;
using Pot.EmailSender;
using Pot.RazorComponents.Models;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Users.Invite;

public class InviteUserServiceFixture : PotFixtureBase
{
    private readonly IPersistableUserRepository _userRepositoryFake = Substitute.For<IPersistableUserRepository>();
    private readonly ISiteRepository _siteRepositoryFake = Substitute.For<ISiteRepository>();
    private readonly IRoleRepository _roleRepositoryFake = Substitute.For<IRoleRepository>();
    private readonly IPreUpdateChecker _preUpdateCheckerFake = Substitute.For<IPreUpdateChecker>();
    private readonly IUserPasswordHasher _passwordHasherFake = Substitute.For<IUserPasswordHasher>();
    private readonly ISendEmailChannelWriter _emailWriterFake = Substitute.For<ISendEmailChannelWriter>();

    public class InviteUserAsync : InviteUserServiceFixture
    {
        [Fact]
        public async Task Should_Name_The_Site_And_The_Inviting_Administrator_In_The_Invitation()
        {
            ArrangeCallerAndSite();

            var input = CreateInput();

            var result = await CreateService().InviteUserAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            await _emailWriterFake.Received(1).SubmitAsync(
                EmailType.Invitation,
                Arg.Is<EmailInvitationInfo>(info => info.Username == "new-user" &&
                    info.DisplayName == "new-user" &&
                    info.Email == "new-user@example.com" &&
                    info.SiteName == "Invite Site" &&
                    info.InvitedByLabel == "Casey Caller (caller)"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Not_Send_An_Invitation_When_The_Username_Is_Taken()
        {
            var (_, site) = ArrangeCallerAndSite();

            var existingUser = EntityFactory.CreateUser(site, "new-user");

            _userRepositoryFake
                .GetByUsernameAsync("new-user", Arg.Any<CancellationToken>())
                .Returns(existingUser);

            var input = CreateInput();

            var result = await CreateService().InviteUserAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();

            await _emailWriterFake.DidNotReceive().SubmitAsync(
                Arg.Any<EmailType>(), Arg.Any<EmailConfigBase>(), Arg.Any<CancellationToken>());
        }

        private (UserEntity Caller, SiteEntity Site) ArrangeCallerAndSite()
        {
            var site = EntityFactory.CreateSite(name: "Invite Site");
            var caller = EntityFactory.CreateUser(site, "caller", "caller@example.com", "Casey Caller");

            _userRepositoryFake.GetCurrentUser(includeSite: false).Returns(caller);
            _siteRepositoryFake.GetCurrentSite().Returns(site);

            _roleRepositoryFake
                .GetRolesAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
                .Returns([]);

            _passwordHasherFake
                .GetHash(Arg.Any<UserEntity>(), Arg.Any<string>())
                .Returns("hashed");

            return (caller, site);
        }

        private static Input CreateInput()
        {
            return new Input
            {
                Username = "new-user",
                Email = "new-user@example.com",
                RoleIds = []
            };
        }
    }

    private InviteUserService CreateService()
    {
        return new InviteUserService(
            _userRepositoryFake,
            _siteRepositoryFake,
            _roleRepositoryFake,
            _preUpdateCheckerFake,
            _passwordHasherFake,
            _emailWriterFake,
            Substitute.For<ILogger<InviteUserService>>());
    }
}
