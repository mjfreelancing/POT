using CommunityToolkit.Mvvm.ComponentModel;
using Pot.Maui.Domain.Accounts.Models;
using Pot.Maui.Domain.Expenses.Models;
using Pot.Maui.Features.Summary.Services;
using Pot.Maui.Mvvm;

namespace Pot.Maui.Features.Summary.ViewModels;

public partial class SummaryViewModel : ViewModelBase
{
    private readonly ISummaryService _summaryService;

    [ObservableProperty]
    private AccountSummary _accountSummary = new();

    [ObservableProperty]
    private ExpenseSummary _expenseSummary = new();

    public SummaryViewModel(ISummaryService summaryService)
    {
        _summaryService = summaryService;

        Title = "Summary";
    }

    //public override async void OnActivate(CompositeDisposable disposables)
    //{
    //    base.OnActivate(disposables);

    //    // TODO: error handling - here or the view - need a service to raise error notifications

    //    using (StartIsBusyScope())
    //    {
    //        AccountSummary = await _summaryService.GetAccountSummaryAsync();

    //        ExpenseSummary = await _summaryService.GetExpenseSummaryAsync();
    //    }
    //}
}
