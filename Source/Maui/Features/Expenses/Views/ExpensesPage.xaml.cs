using Pot.Maui.Features.Expenses.ViewModels;
using Pot.Maui.Mvvm;

namespace Pot.Maui.Features.Expenses.Views;

public partial class ExpensesPage : ViewModelContentPage<ExpensesViewModel>
{
    public ExpensesPage(ExpensesViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}