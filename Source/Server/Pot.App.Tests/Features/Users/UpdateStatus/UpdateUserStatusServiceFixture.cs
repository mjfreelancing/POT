using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Users.UpdateStatus;
using Pot.App.Features.Users.UpdateStatus.EntityChecks;
using Pot.App.Features.Users.UpdateStatus.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Users;
using Pot.Shared.Enumerations;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Users.UpdateStatus;

public class UpdateUserStatusServiceFixture : PotFixtureBase
{
    public class Constructor : UpdateUserStatusServiceFixture
    {
        private readonly IPersistableUserRepository _userRepositoryFake;
        private readonly IPreUpdateChecker _preUpdateCheckerFake;

        public Constructor()
        {
            _userRepositoryFake = Substitute.For<IPersistableUserRepository>();
            _preUpdateCheckerFake = Substitute.For<IPreUpdateChecker>();
        }

        [Fact]
        public void Should_Throw_When_UserRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<UpdateUserStatusService>>();

                _ = new UpdateUserStatusService(null!, _preUpdateCheckerFake, logger);
            });

            exception.ParamName.ShouldBe("userRepository");
        }

        [Fact]
        public void Should_Throw_When_PreUpdateChecker_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<UpdateUserStatusService>>();

                _ = new UpdateUserStatusService(_userRepositoryFake, null!, logger);
            });

            exception.ParamName.ShouldBe("preUpdateChecker");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new UpdateUserStatusService(_userRepositoryFake, _preUpdateCheckerFake, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class UpdateUserStatusAsync : UpdateUserStatusServiceFixture
    {
        private readonly IPersistableUserRepository _userRepositoryFake;
        private readonly IPreUpdateChecker _preUpdateCheckerFake;

        public UpdateUserStatusAsync()
        {
            _userRepositoryFake = Substitute.For<IPersistableUserRepository>();
            _preUpdateCheckerFake = Substitute.For<IPreUpdateChecker>();
        }

        [Fact]
        public async Task Should_Fail_And_Not_Write_When_The_Target_Is_Not_In_The_Callers_Site()
        {
            _userRepositoryFake
                .GetForCurrentSiteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((UserEntity?)null);

            var service = CreateService();
            var input = CreateInput(Guid.NewGuid());

            var result = await service.UpdateUserStatusAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();

            await _userRepositoryFake.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Fail_And_Not_Write_When_The_PreUpdateChecker_Refuses()
        {
            var user = EntityFactory.CreateUser(EntityFactory.CreateSite(), "same-site-user");
            var checkerError = ApiDetailErrorFactory.CreateEntityConstraintError("Status", "Status cannot be changed");

            _userRepositoryFake
                .GetForCurrentSiteAsync(user.RowId, Arg.Any<CancellationToken>())
                .Returns(user);

            _preUpdateCheckerFake
                .CanSaveAsync(Arg.Any<Input>(), Arg.Any<UserEntity>(), Arg.Any<CancellationToken>())
                .Returns(checkerError);

            var service = CreateService();
            var input = CreateInput(user.RowId);

            var result = await service.UpdateUserStatusAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();

            await _userRepositoryFake.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Update_The_Status_When_The_Target_Is_In_The_Callers_Site()
        {
            var user = EntityFactory.CreateUser(EntityFactory.CreateSite(), "same-site-user");

            _userRepositoryFake
                .GetForCurrentSiteAsync(user.RowId, Arg.Any<CancellationToken>())
                .Returns(user);

            var service = CreateService();
            var input = CreateInput(user.RowId);

            var result = await service.UpdateUserStatusAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();

            user.Status.ShouldBe(UserStatus.Disabled);

            await _userRepositoryFake.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        private UpdateUserStatusService CreateService()
        {
            var logger = Substitute.For<ILogger<UpdateUserStatusService>>();

            return new UpdateUserStatusService(_userRepositoryFake, _preUpdateCheckerFake, logger);
        }

        private static Input CreateInput(Guid rowId)
        {
            return new Input
            {
                RowId = rowId,
                Etag = 1,
                Status = UserStatus.Disabled,
                PlatformAdminRowIds = []
            };
        }
    }
}
