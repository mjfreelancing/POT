using Microsoft.Extensions.Logging;
using NSubstitute;
using Pot.App.Errors;
using Pot.App.Features.Accounts.Create;
using Pot.App.Features.Accounts.Create.EntityChecks;
using Pot.App.Features.Accounts.Create.Models;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Sites;
using Pot.TestUtils;
using Shouldly;

namespace Pot.App.Tests.Features.Accounts.Create;

public class CreateAccountServiceFixture : PotFixtureBase
{
    private sealed class NoopScope : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class CreateAccountAsync : CreateAccountServiceFixture
    {
        private readonly IPersistableAccountRepository _accountRepositoryFake;
        private readonly ISiteRepository _siteRepositoryFake;
        private readonly IPreCreateChecker _preCreateCheckerFake;
        private readonly ILogger<CreateAccountService> _loggerFake;

        public CreateAccountAsync()
        {
            _accountRepositoryFake = Substitute.For<IPersistableAccountRepository>();
            _siteRepositoryFake = Substitute.For<ISiteRepository>();
            _preCreateCheckerFake = Substitute.For<IPreCreateChecker>();
            _loggerFake = Substitute.For<ILogger<CreateAccountService>>();

            _accountRepositoryFake.WithTracking().Returns(new NoopScope());
            _siteRepositoryFake.GetCurrentSite().Returns(EntityFactory.CreateSite());
            _preCreateCheckerFake
                .CanSaveAsync(Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>())
                .Returns((ApiDetailError?)null);
            _accountRepositoryFake
                .AddAndSaveAsync(Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>())
                .Returns(1);
        }

        [Fact]
        public async Task Should_Store_A_Three_Decimal_Balance_And_Reserved_As_Two_Decimals()
        {
            AccountEntity? capturedAccount = null;

            _accountRepositoryFake
                .AddAndSaveAsync(Arg.Do<AccountEntity>(account => capturedAccount = account), Arg.Any<CancellationToken>())
                .Returns(1);

            var service = new CreateAccountService(_accountRepositoryFake, _siteRepositoryFake, _preCreateCheckerFake,
                _loggerFake);

            // A double holding 10.005 holds a little less than that, so it only becomes 10.01 when the rounding reads
            // the decimal the value represents rather than its binary approximation.
            var input = new Input
            {
                Description = "Everyday",
                Balance = 10.005d,
                Reserved = 2.005d
            };

            var result = await service.CreateAccountAsync(input, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            capturedAccount.ShouldNotBeNull();
            capturedAccount.Balance.ShouldBe(10.01d);
            capturedAccount.Reserved.ShouldBe(2.01d);
        }

        [Fact]
        public async Task Should_Store_An_Imported_Account_With_Two_Decimals_And_Its_RowId()
        {
            AccountEntity? capturedAccount = null;

            _accountRepositoryFake
                .AddAndSaveAsync(Arg.Do<AccountEntity>(account => capturedAccount = account), Arg.Any<CancellationToken>())
                .Returns(1);

            var service = new CreateAccountService(_accountRepositoryFake, _siteRepositoryFake, _preCreateCheckerFake,
                _loggerFake);

            // The importer supplies the row id so the account's expenses and incomes can be linked to it afterwards.
            var importedRowId = Guid.NewGuid();

            var input = new Input
            {
                RowId = importedRowId,
                Description = "Everyday",
                Balance = 10.005d,
                Reserved = 2.005d
            };

            var result = await service.CreateAccountAsync(input, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            capturedAccount.ShouldNotBeNull();
            capturedAccount.RowId.ShouldBe(importedRowId);
            capturedAccount.Balance.ShouldBe(10.01d);
            capturedAccount.Reserved.ShouldBe(2.01d);
        }
    }
}
