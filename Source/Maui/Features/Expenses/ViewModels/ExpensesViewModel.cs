using Pot.Maui.Domain.Expenses.Models;
using Pot.Maui.Domain.Expenses.Repository;
using Pot.Maui.Helpers;
using Pot.Maui.Mvvm;
using System.Reactive.Disposables;

namespace Pot.Maui.Features.Expenses.ViewModels;

public partial class ExpensesViewModel : ViewModelBase
{
    private readonly IExpenseRepository _expenseRepository;

    public ObservableRangeCollection<Expense> Expenses { get; } = [];

    public ExpensesViewModel(IExpenseRepository accountRepository)
    {
        _expenseRepository = accountRepository;

        Title = "Expenses";
    }

    public override async void OnActivate(CompositeDisposable disposables)
    {
        base.OnActivate(disposables);

        using (StartIsBusyScope())
        {
            var allExpenses = await _expenseRepository.GetAllExpensesAsync();

            Expenses.ReplaceRange(allExpenses);
        }
    }
}
