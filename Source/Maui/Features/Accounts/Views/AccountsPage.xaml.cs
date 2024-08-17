using Pot.Maui.Features.Accounts.ViewModels;
using Pot.Maui.Mvvm;

namespace Pot.Maui.Features.Accounts.Views;

public partial class AccountsPage : ViewModelContentPage<AccountsViewModel>
{
    private IDisposable? _subscription1;

    public AccountsPage(AccountsViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        // The view model may kick off operations when it is activated (when base.OnAppearing() is called),
        // so observables are setup in advance.

        _subscription1 = ViewModel.BusyChanged
            .Subscribe(busy =>
            {

            });

        // Now allow the view model to be 'activated'
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _subscription1!.Dispose();
        _subscription1 = null;
    }
}