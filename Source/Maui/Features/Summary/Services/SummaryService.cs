using Pot.Features.Calculators.Allocation;
using Pot.Features.Calculators.Summary;
using Pot.Maui.Domain.Accounts.Models;
using Pot.Maui.Domain.Accounts.Repository;
using Pot.Maui.Domain.Expenses.Models;
using Pot.Maui.Domain.Expenses.Repository;

namespace Pot.Maui.Features.Summary.Services;

public interface ISummaryService
{
    Task<AccountSummary> GetAccountSummaryAsync();
    Task<ExpenseSummary> GetExpenseSummaryAsync();
}

internal sealed class SummaryService : ISummaryService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IAllocationCalculator _allocationCalculator;

    public SummaryService(IAccountRepository accountRepository, IExpenseRepository expenseRepository,
        IAllocationCalculator allocationCalculator)
    {
        _accountRepository = accountRepository;
        _expenseRepository = expenseRepository;
        _allocationCalculator = allocationCalculator;
    }

    public async Task<AccountSummary> GetAccountSummaryAsync()
    {
        //Logger.Debug("Updating account summary");

        var accounts = await _accountRepository.GetAllAccountsAsync();

        return SummaryCalculator.GetAccountSummary(accounts);
    }

    public async Task<ExpenseSummary> GetExpenseSummaryAsync()
    {
        //Logger.Debug("Updating expense summary");

        var expenses = await _expenseRepository.GetAllExpensesAsync();

        return SummaryCalculator.GetExpenseSummary(expenses);
    }
}
