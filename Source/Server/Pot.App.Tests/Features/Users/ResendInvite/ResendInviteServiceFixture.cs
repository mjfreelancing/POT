using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Concerns.Auth;
using Pot.App.Features.Users.ResendInvite;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.EmailSender;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Users.ResendInvite;

public class ResendInviteServiceFixture : PotFixtureBase
{
    public class Constructor : ResendInviteServiceFixture
    {
        private readonly IPersistableUserRepository _userRepositoryFake;
        private readonly IUserPasswordHasher _passwordHasherFake;
        private readonly ISendEmailChannelWriter _sendEmailChannelWriterFake;

        public Constructor()
        {
            _userRepositoryFake = Substitute.For<IPersistableUserRepository>();
            _passwordHasherFake = Substitute.For<IUserPasswordHasher>();
            _sendEmailChannelWriterFake = Substitute.For<ISendEmailChannelWriter>();
        }

        [Fact]
        public void Should_Throw_When_UserRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<ResendInviteService>>();

                _ = new ResendInviteService(null!, _passwordHasherFake, _sendEmailChannelWriterFake, logger);
            });

            exception.ParamName.ShouldBe("userRepository");
        }

        [Fact]
        public void Should_Throw_When_PasswordHasher_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<ResendInviteService>>();

                _ = new ResendInviteService(_userRepositoryFake, null!, _sendEmailChannelWriterFake, logger);
            });

            exception.ParamName.ShouldBe("passwordHasher");
        }

        [Fact]
        public void Should_Throw_When_SendEmailChannelWriter_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<ResendInviteService>>();

                _ = new ResendInviteService(_userRepositoryFake, _passwordHasherFake, null!, logger);
            });

            exception.ParamName.ShouldBe("sendEmailChannelWriter");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new ResendInviteService(_userRepositoryFake, _passwordHasherFake, _sendEmailChannelWriterFake, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class ResendInviteAsync : ResendInviteServiceFixture
    {
        private readonly IPersistableUserRepository _userRepositoryFake;
        private readonly IUserPasswordHasher _passwordHasherFake;
        private readonly ISendEmailChannelWriter _sendEmailChannelWriterFake;

        public ResendInviteAsync()
        {
            _userRepositoryFake = Substitute.For<IPersistableUserRepository>();
            _passwordHasherFake = Substitute.For<IUserPasswordHasher>();
            _sendEmailChannelWriterFake = Substitute.For<ISendEmailChannelWriter>();
        }

        [Fact]
        public async Task Should_Fail_And_Not_Write_When_The_Target_Is_Not_In_The_Callers_Site()
        {
            _userRepositoryFake
                .GetForCurrentSiteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((UserEntity?)null);

            var service = CreateService();

            var result = await service.ResendInviteAsync(Guid.NewGuid(), CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();

            await _userRepositoryFake.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Fail_And_Not_Write_When_The_User_Is_Not_Pending()
        {
            // Seeded users are enabled, so an invitation cannot be outstanding for them.
            var user = EntityFactory.CreateUser(EntityFactory.CreateSite(), "enabled-user");

            _userRepositoryFake
                .GetForCurrentSiteAsync(user.RowId, Arg.Any<CancellationToken>())
                .Returns(user);

            var service = CreateService();

            var result = await service.ResendInviteAsync(user.RowId, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();

            await _userRepositoryFake.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Resend_The_Invitation_When_The_User_Is_Pending()
        {
            var user = EntityFactory.CreateUser(EntityFactory.CreateSite(), "pending-user");
            user.Status = UserStatus.Pending;

            _passwordHasherFake
                .GetHash(user, Arg.Any<string>())
                .Returns("hashed");

            _userRepositoryFake
                .GetForCurrentSiteAsync(user.RowId, Arg.Any<CancellationToken>())
                .Returns(user);

            var service = CreateService();

            var result = await service.ResendInviteAsync(user.RowId, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            await _userRepositoryFake.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        private ResendInviteService CreateService()
        {
            var logger = Substitute.For<ILogger<ResendInviteService>>();

            return new ResendInviteService(_userRepositoryFake, _passwordHasherFake, _sendEmailChannelWriterFake, logger);
        }
    }
}
