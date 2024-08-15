using Pot.Maui.Domain.Expenses.Models;
using Pot.Maui.Domain.Expenses.Repository;
using Pot.Maui.Helpers;

namespace Pot.Maui.Features.Expenses.ViewModels;

//public interface IExpensesViewModel : IViewModel
//{
//    ObservableRangeCollection<Expense> Expenses { get; }

//    Task RefreshAsync();
//}

public partial class ExpensesViewModel : ViewModelBase//, IExpensesViewModel
{
    private readonly IExpenseRepository _expenseRepository;

    public ObservableRangeCollection<Expense> Expenses { get; } = [];

    public ExpensesViewModel(IExpenseRepository accountRepository)
    {
        _expenseRepository = accountRepository;

        Title = "Expenses";
    }

    public async Task RefreshAsync()
    {
        var allExpenses = await _expenseRepository.GetAllExpensesAsync();

        Expenses.ReplaceRange(allExpenses);
    }
}
