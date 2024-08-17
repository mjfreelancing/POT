using System.Reactive.Disposables;

namespace Pot.Maui.Mvvm;

public abstract class ViewModelContentPage<TViewModel> : ContentPage where TViewModel : ViewModelBase
{
    private CompositeDisposable? _disposables = null;

    protected TViewModel ViewModel { get; }

    public ViewModelContentPage(TViewModel viewModel)
    {
        BindingContext = viewModel;
        ViewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _disposables = [];

        ViewModel.OnActivate(_disposables);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ViewModel.OnDeactivate();

        _disposables!.Dispose();
        _disposables = null;
    }
}
