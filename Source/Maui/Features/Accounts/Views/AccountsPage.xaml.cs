using Pot.Maui.Features.Accounts.ViewModels;

namespace Pot.Maui.Features.Accounts.Views;

public partial class AccountsPage : ContentPage
{
    private readonly AccountsViewModel _accountsViewModel;

    public AccountsPage(AccountsViewModel accountsViewModel)
    {
        InitializeComponent();

        BindingContext = accountsViewModel;

        _accountsViewModel = accountsViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _accountsViewModel.RefreshAsync();
    }
}