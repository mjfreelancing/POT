using Pot.Maui.Features.Accounts.ViewModels;
using Pot.Maui.Mvvm;

namespace Pot.Maui.Features.Accounts.Views;

public partial class AccountsPage : ViewModelContentPage<AccountsViewModel>
{
    public AccountsPage(AccountsViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}