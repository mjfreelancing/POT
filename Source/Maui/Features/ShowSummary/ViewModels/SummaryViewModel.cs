using CommunityToolkit.Mvvm.ComponentModel;
using Pot.Maui.Domain.Accounts.Models;
using Pot.Maui.Domain.Expenses.Models;
using Pot.Maui.Features.ShowSummary.Services;

namespace Pot.Maui.Features.ShowSummary.ViewModels;

public interface ISummaryViewModel
{
    AccountSummary AccountSummary { get; }
    ExpenseSummary ExpenseSummary { get; }

    Task RefreshSummariesAsync();
}

internal partial class SummaryViewModel : ViewModelBase, ISummaryViewModel
{
    private readonly ISummaryService _summaryService;

    [ObservableProperty]
    private AccountSummary _accountSummary = new();

    [ObservableProperty]
    private ExpenseSummary _expenseSummary = new();

    public SummaryViewModel(ISummaryService summaryService)
    {
        _summaryService = summaryService;
    }

    public async Task RefreshSummariesAsync()
    {
        // TODO: error handling - here or the view - need a service to raise error notifications

        AccountSummary = await _summaryService.GetAccountSummaryAsync();

        ExpenseSummary = await _summaryService.GetExpenseSummaryAsync();
    }
}
