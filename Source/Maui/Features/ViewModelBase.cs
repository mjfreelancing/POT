using AllOverIt.Patterns.ResourceInitialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pot.Maui.Features;

public partial class ViewModelBase : ObservableObject, IViewModel
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    public IDisposable GetIsBusyTransaction()
    {
        // TODO: Create a reference counted version so it can handle nested calls
        return new Raii(
            () => { IsBusy = true; },
            () => { IsBusy = false; });
    }
}
