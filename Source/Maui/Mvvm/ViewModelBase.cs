using AllOverIt.Patterns.ResourceInitialization;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Reactive.Disposables;

namespace Pot.Maui.Mvvm;

public partial class ViewModelBase : ObservableObject, IViewModel
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    // Is observable, indirectly via IsBusy
    public bool IsNotBusy => !IsBusy;

    // TODO: These are disposable. Do we bother?
    public ObservableProperty<bool> BusyChanging { get; } = new();  // Observes IsBusy PropertyChanging
    public ObservableProperty<bool> BusyChanged { get; } = new();  // Observes IsBusy PropertyChanged

    public virtual void OnActivate(CompositeDisposable disposables)
    {
        // The view is appearing, so setup the observables so they start monitoring property changes.
        // The observables are cleaned up when the view is disappearing.
        BusyChanging.BindToPropertyChanging(this, vm => vm.IsBusy, disposables);
        BusyChanged.BindToPropertyChanged(this, vm => vm.IsBusy, disposables);
    }

    public virtual void OnDeactivate()
    {
    }
    public IDisposable StartIsBusyScope()
    {
        // TODO: Create a reference counted version so it can handle nested calls
        return new Raii(
            () => { IsBusy = true; },
            () => { IsBusy = false; });
    }
}
