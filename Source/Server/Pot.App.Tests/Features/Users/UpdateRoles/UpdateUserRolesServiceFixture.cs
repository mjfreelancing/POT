using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Features.Users.UpdateRoles;
using Pot.App.Features.Users.UpdateRoles.EntityChecks;
using Pot.App.Features.Users.UpdateRoles.Models;
using Pot.Data;
using Pot.Data.Entities;
using Pot.Data.Repositories.Roles;
using Pot.Data.Repositories.Users;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Users.UpdateRoles;

public class UpdateUserRolesServiceFixture : PotFixtureBase
{
    public class Constructor : UpdateUserRolesServiceFixture
    {
        private readonly IPersistableUserRepository _userRepositoryFake;
        private readonly IRoleRepository _roleRepositoryFake;
        private readonly IPotTransactionFactory _transactionFactoryFake;
        private readonly IPreUpdateChecker _preUpdateCheckerFake;

        public Constructor()
        {
            _userRepositoryFake = Substitute.For<IPersistableUserRepository>();
            _roleRepositoryFake = Substitute.For<IRoleRepository>();
            _transactionFactoryFake = Substitute.For<IPotTransactionFactory>();
            _preUpdateCheckerFake = Substitute.For<IPreUpdateChecker>();
        }

        [Fact]
        public void Should_Throw_When_UserRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<UpdateUserRolesService>>();

                _ = new UpdateUserRolesService(null!, _roleRepositoryFake, _transactionFactoryFake, _preUpdateCheckerFake, logger);
            });

            exception.ParamName.ShouldBe("userRepository");
        }

        [Fact]
        public void Should_Throw_When_RoleRepository_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<UpdateUserRolesService>>();

                _ = new UpdateUserRolesService(_userRepositoryFake, null!, _transactionFactoryFake, _preUpdateCheckerFake, logger);
            });

            exception.ParamName.ShouldBe("roleRepository");
        }

        [Fact]
        public void Should_Throw_When_TransactionFactory_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<UpdateUserRolesService>>();

                _ = new UpdateUserRolesService(_userRepositoryFake, _roleRepositoryFake, null!, _preUpdateCheckerFake, logger);
            });

            exception.ParamName.ShouldBe("transactionFactory");
        }

        [Fact]
        public void Should_Throw_When_PreUpdateChecker_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                var logger = Substitute.For<ILogger<UpdateUserRolesService>>();

                _ = new UpdateUserRolesService(_userRepositoryFake, _roleRepositoryFake, _transactionFactoryFake, null!, logger);
            });

            exception.ParamName.ShouldBe("preUpdateChecker");
        }

        [Fact]
        public void Should_Throw_When_Logger_Is_Null()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
            {
                _ = new UpdateUserRolesService(_userRepositoryFake, _roleRepositoryFake, _transactionFactoryFake, _preUpdateCheckerFake, null!);
            });

            exception.ParamName.ShouldBe("logger");
        }
    }

    public class UpdateUserRolesAsync : UpdateUserRolesServiceFixture
    {
        private readonly IPersistableUserRepository _userRepositoryFake;
        private readonly IRoleRepository _roleRepositoryFake;
        private readonly IPotTransactionFactory _transactionFactoryFake;
        private readonly IPreUpdateChecker _preUpdateCheckerFake;

        public UpdateUserRolesAsync()
        {
            _userRepositoryFake = Substitute.For<IPersistableUserRepository>();
            _roleRepositoryFake = Substitute.For<IRoleRepository>();
            _transactionFactoryFake = Substitute.For<IPotTransactionFactory>();
            _preUpdateCheckerFake = Substitute.For<IPreUpdateChecker>();
        }

        [Fact]
        public async Task Should_Fail_And_Not_Write_When_The_Target_Is_Not_In_The_Callers_Site()
        {
            _userRepositoryFake
                .GetForCurrentSiteWithRolesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((UserEntity?)null);

            var service = CreateService();
            var input = CreateInput(Guid.NewGuid());

            var result = await service.UpdateUserRolesAsync(input, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();

            await _userRepositoryFake.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
        }

        // The same-site success path is deliberately not unit-tested here: the service requires a tracked user and
        // reads its change-tracker entry, which a substitute cannot supply. The acceptance suite covers it end to
        // end, including that the roles change is written.
        private UpdateUserRolesService CreateService()
        {
            var logger = Substitute.For<ILogger<UpdateUserRolesService>>();

            return new UpdateUserRolesService(_userRepositoryFake, _roleRepositoryFake, _transactionFactoryFake, _preUpdateCheckerFake, logger);
        }

        private static Input CreateInput(Guid rowId)
        {
            return new Input
            {
                RowId = rowId,
                Etag = 1,
                RoleIds = [Guid.NewGuid()]
            };
        }
    }
}
