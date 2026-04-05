using Pot.Data.Entities;
using Pot.Shared.Enumerations;

namespace Pot.TestUtils;

public static class EntityFactory
{
    public static SiteEntity CreateSite(string? name = null, string? description = null)
    {
        return new SiteEntity
        {
            RowId = Guid.NewGuid(),
            Name = name ?? "Test Site",
            Description = description ?? "Test Description"
        };
    }

    public static UserEntity CreateUser(SiteEntity site, string? username = null, string? email = null, string? displayName = null)
    {
        return new UserEntity
        {
            RowId = Guid.NewGuid(),
            Username = username ?? "testuser",
            Email = email ?? "test@example.com",
            DisplayName = displayName ?? "Test User",
            Status = UserStatus.Enabled,
            PasswordHash = "hash",
            Site = site
        };
    }

    public static AccountEntity CreateAccount(SiteEntity site, string description, double balance, double reserved = 0.0d)
    {
        return new AccountEntity
        {
            RowId = Guid.NewGuid(),
            Description = description,
            Bsb = "123-456",
            Number = "12345678",
            Balance = balance,
            Reserved = reserved,
            TotalExpenseAccrued = 0.0d,
            DailyExpenseAccrual = 0.0d,
            Site = site,
            Expenses = [],
            Incomes = []
        };
    }

    public static ExpenseEntity CreateExpense(AccountEntity account, bool excludeFromCalc, string description, double amount,
        string accrualStart, string nextDue, string? endDate, Frequency frequency, int frequencyCount, AccrualPolicy? accrualPolicy = null)
    {
        var expense = new ExpenseEntity
        {
            RowId = Guid.NewGuid(),
            ExcludeFromCalcs = excludeFromCalc,
            Account = account,
            Description = description,
            Amount = amount,
            AccrualStart = DateOnly.ParseExact(accrualStart, "yyyy-MM-dd"),
            NextDue = DateOnly.ParseExact(nextDue, "yyyy-MM-dd"),
            EndDate = endDate is null ? null : DateOnly.ParseExact(endDate, "yyyy-MM-dd"),
            AccrualPolicy = accrualPolicy ?? (frequency == Frequency.OneTime ? AccrualPolicy.None : AccrualPolicy.Automatic),
            Frequency = frequency,
            FrequencyCount = frequencyCount,
            Accrued = 0.0d,
            AccruedIsDirty = true,
            LastAccruedUpdate = null
        };

        return expense;
    }

    public static IncomeEntity CreateIncome(AccountEntity account, bool excludeFromCalc, string description, double amount,
        string nextDue, string? endDate, Frequency frequency, int frequencyCount)
    {
        var income = new IncomeEntity
        {
            RowId = Guid.NewGuid(),
            ExcludeFromCalcs = excludeFromCalc,
            Account = account,
            Description = description,
            Amount = amount,
            NextDue = DateOnly.ParseExact(nextDue, "yyyy-MM-dd"),
            EndDate = endDate is null ? null : DateOnly.ParseExact(endDate, "yyyy-MM-dd"),
            Frequency = frequency,
            FrequencyCount = frequencyCount
        };

        return income;
    }
}
