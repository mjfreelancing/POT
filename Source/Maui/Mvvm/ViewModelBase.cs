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

    public bool IsNotBusy => !IsBusy;

    public IDisposable StartIsBusyScope()
    {
        // TODO: Create a reference counted version so it can handle nested calls
        return new Raii(
            () => { IsBusy = true; },
            () => { IsBusy = false; });
    }

    public virtual void OnActivate(CompositeDisposable disposables)
    {
    }

    public virtual void OnDeactivate()
    {
    }
}
