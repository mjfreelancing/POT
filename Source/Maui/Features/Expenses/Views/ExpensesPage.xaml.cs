using Pot.Maui.Features.Expenses.ViewModels;

namespace Pot.Maui.Features.Expenses.Views;

public partial class ExpensesPage : ContentPage
{
    private readonly ExpensesViewModel _expensesViewModel;

    public ExpensesPage(ExpensesViewModel expensesViewModel)
    {
        InitializeComponent();

        BindingContext = expensesViewModel;

        _expensesViewModel = expensesViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _expensesViewModel.RefreshAsync();
    }
}