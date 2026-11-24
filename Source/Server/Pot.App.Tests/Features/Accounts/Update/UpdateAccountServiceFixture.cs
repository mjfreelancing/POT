using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Accounts.Update;
using Pot.App.Features.Accounts.Update.EntityChecks;
using Pot.App.Features.Accounts.Update.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Accounts.Update;

public class UpdateAccountServiceFixture : PotFixtureBase
{
    private sealed class NoopScope : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class UpdateAccountAsync : UpdateAccountServiceFixture
    {
        private readonly IPersistableAccountRepository _accountRepositoryFake;
        private readonly IPreUpdateChecker _preUpdateCheckerFake;
        private readonly ILogger<UpdateAccountService> _loggerFake;

        public UpdateAccountAsync()
        {
            _accountRepositoryFake = Substitute.For<IPersistableAccountRepository>();
            _preUpdateCheckerFake = Substitute.For<IPreUpdateChecker>();
            _loggerFake = Substitute.For<ILogger<UpdateAccountService>>();

            _accountRepositoryFake.WithTracking().Returns(new NoopScope());
            _preUpdateCheckerFake
                .CanSaveAsync(Arg.Any<Input>(), Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>())
                .Returns((ApiDetailError?)null);
            _accountRepositoryFake
                .SaveAsync(Arg.Any<CancellationToken>())
                .Returns(1);
        }

        [Fact]
        public async Task Should_Store_A_Three_Decimal_Balance_And_Reserved_As_Two_Decimals()
        {
            var existingAccount = EntityFactory.CreateAccount(EntityFactory.CreateSite(), "Everyday", 1000.0d);

            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(existingAccount);

            var service = new UpdateAccountService(_accountRepositoryFake, _preUpdateCheckerFake, _loggerFake);

            // A double holding 10.005 holds a little less than that, so it only becomes 10.01 when the rounding reads
            // the decimal the value represents rather than its binary approximation.
            var input = new Input
            {
                RowId = existingAccount.RowId,
                Etag = existingAccount.Etag,
                Description = "Everyday",
                Balance = 10.005d,
                Reserved = 2.005d
            };

            var result = await service.UpdateAccountAsync(input, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            existingAccount.Balance.ShouldBe(10.01d);
            existingAccount.Reserved.ShouldBe(2.01d);
        }

        [Fact]
        public async Task Should_Fail_When_Account_Does_Not_Exist()
        {
            _accountRepositoryFake
                .GetAccountOrDefaultAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((AccountEntity?)null);

            var service = new UpdateAccountService(_accountRepositoryFake, _preUpdateCheckerFake, _loggerFake);

            var input = new Input
            {
                RowId = Guid.NewGuid(),
                Etag = 1,
                Description = "Everyday"
            };

            var result = await service.UpdateAccountAsync(input, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeFalse();

            await _accountRepositoryFake
                .DidNotReceive()
                .SaveAsync(Arg.Any<CancellationToken>());
        }
    }
}
