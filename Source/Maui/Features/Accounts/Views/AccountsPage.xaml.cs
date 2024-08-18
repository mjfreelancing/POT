using AllOverIt.Reactive.Extensions;
using Pot.Maui.Features.Accounts.ViewModels;
using Pot.Maui.Mvvm;
using System.Reactive.Disposables;

namespace Pot.Maui.Features.Accounts.Views;

public partial class AccountsPage : ViewModelContentPage<AccountsViewModel>
{
    private CompositeDisposable? _disposables;
    private IDisposable? _subscription1;
    private IDisposable? _subscription2;

    public AccountsPage(AccountsViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        // The view model may kick off operations when it is activated (when base.OnAppearing() is called),
        // so observables are setup in advance.

        _subscription1 = ViewModel.BusyChanging
            .Subscribe(busy =>
            {

            });

        _subscription2 = ViewModel.BusyChanged
            .Subscribe(busy =>
            {
            });



        _disposables = [];

        var isBusyChanged = new ObservableProperty<bool>();
        isBusyChanged.BindToPropertyChanged(ViewModel, vm => vm.IsBusy, _disposables);

        isBusyChanged
            .Subscribe(busy =>
            {
            })
            .DisposeUsing(_disposables);



        // Now allow the view model to be 'activated'
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _subscription1!.Dispose();
        _subscription1 = null;

        _subscription2!.Dispose();
        _subscription2 = null;

        _disposables?.Dispose();
        _disposables = null;
    }
}