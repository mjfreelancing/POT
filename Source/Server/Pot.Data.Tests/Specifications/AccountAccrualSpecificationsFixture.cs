using Pot.Data.Entities;
using Pot.Data.Specifications;
using Pot.TestUtils;
using Shouldly;

namespace Pot.Data.Tests.Specifications;

public class AccountAccrualSpecificationsFixture : PotFixtureBase
{
    private readonly SiteEntity _site;
    private readonly AccountEntity _account;

    public AccountAccrualSpecificationsFixture()
    {
        _site = EntityFactory.CreateSite();
        _account = EntityFactory.CreateAccount(_site, "Test Account", 0.0d);
    }

    public class IsInAccountSet : AccountAccrualSpecificationsFixture
    {
        [Fact]
        public void Should_Return_True_When_Account_Is_In_Set()
        {
            var accountAccrual = CreateAccountAccrual(accruedIsDirty: false, lastAccruedDate: new DateOnly(2026, 4, 24));

            var specification = AccountAccrualSpecifications.IsInAccountSet([_account.RowId]);
            var result = specification.IsSatisfiedBy(accountAccrual);

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_False_When_Account_Is_Not_In_Set()
        {
            var otherAccount = EntityFactory.CreateAccount(_site, "Other Account", 0.0d);
            var accountAccrual = new AccountAccrualEntity
            {
                Account = otherAccount,
                AccruedIsDirty = false,
                LastAccruedDate = new DateOnly(2026, 4, 24)
            };

            var specification = AccountAccrualSpecifications.IsInAccountSet([_account.RowId]);
            var result = specification.IsSatisfiedBy(accountAccrual);

            result.ShouldBeFalse();
        }
    }

    public class RequiresAccrualUpdate : AccountAccrualSpecificationsFixture
    {
        [Fact]
        public void Should_Return_True_When_AccruedIsDirty_Is_True()
        {
            var asOfDate = new DateOnly(2026, 4, 24);
            var accountAccrual = CreateAccountAccrual(accruedIsDirty: true, lastAccruedDate: asOfDate);

            var specification = AccountAccrualSpecifications.RequiresAccrualUpdate(asOfDate);
            var result = specification.IsSatisfiedBy(accountAccrual);

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_True_When_LastAccruedDate_Is_Null()
        {
            var asOfDate = new DateOnly(2026, 4, 24);
            var accountAccrual = CreateAccountAccrual(accruedIsDirty: false, lastAccruedDate: null);

            var specification = AccountAccrualSpecifications.RequiresAccrualUpdate(asOfDate);
            var result = specification.IsSatisfiedBy(accountAccrual);

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_True_When_LastAccruedDate_Is_Before_AsOfDate()
        {
            var asOfDate = new DateOnly(2026, 4, 24);
            var accountAccrual = CreateAccountAccrual(accruedIsDirty: false, lastAccruedDate: asOfDate.AddDays(-1));

            var specification = AccountAccrualSpecifications.RequiresAccrualUpdate(asOfDate);
            var result = specification.IsSatisfiedBy(accountAccrual);

            result.ShouldBeTrue();
        }

        [Fact]
        public void Should_Return_False_When_All_Conditions_Are_False()
        {
            var asOfDate = new DateOnly(2026, 4, 24);
            var accountAccrual = CreateAccountAccrual(accruedIsDirty: false, lastAccruedDate: asOfDate);

            var specification = AccountAccrualSpecifications.RequiresAccrualUpdate(asOfDate);
            var result = specification.IsSatisfiedBy(accountAccrual);

            result.ShouldBeFalse();
        }
    }

    private AccountAccrualEntity CreateAccountAccrual(bool accruedIsDirty, DateOnly? lastAccruedDate)
    {
        return new AccountAccrualEntity
        {
            Account = _account,
            AccruedIsDirty = accruedIsDirty,
            LastAccruedDate = lastAccruedDate
        };
    }
}
