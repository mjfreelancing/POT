using Pot.Data.Entities;
using Pot.Shared.Enumerations;
using Pot.TestUtils;

namespace Pot.App.Tests.Calculators;

public class CalculatorFixtureBase : PotFixtureBase
{
    protected ExpenseEntity CreateExpense(AccountEntity account, bool excludeFromCalc, string description, double amount,
    string accrualStart, string nextDue, string? endDate, Frequency frequency, int frequencyCount)
    {

        var expense = Create<ExpenseEntity>();

        expense.ExcludeFromCalcs = excludeFromCalc;
        expense.Account = account;
        expense.Description = description;
        expense.Amount = amount;
        expense.AccrualStart = DateOnly.ParseExact(accrualStart, "yyyy-MM-dd");
        expense.NextDue = DateOnly.ParseExact(nextDue, "yyyy-MM-dd");
        expense.EndDate = endDate is null ? null : DateOnly.ParseExact(endDate, "yyyy-MM-dd");
        expense.Frequency = frequency;
        expense.FrequencyCount = frequencyCount;

        return expense;
    }
}
