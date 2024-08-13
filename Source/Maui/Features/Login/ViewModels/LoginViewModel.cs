using CommunityToolkit.Mvvm.Input;

namespace Pot.Maui.Features.Login.ViewModels;

internal partial class LoginViewModel : ViewModelBase, ILoginViewModel
{
    //private readonly BehaviorSubject<bool> _loggedInSubject = new(false);

    //public bool IsLoggedIn => _loggedInSubject.Value;
    //public IObservable<bool> LoggedInStatusChanged => _loggedInSubject;

    public event EventHandler? OnLoggedIn;

    public LoginViewModel()
    {
    }

    [RelayCommand]
    private async Task<bool> LoginAsync()
    {
        using (GetIsBusyTransaction())
        {
            await Task.Delay(1500);

            RaiseOnLoggedIn();
            //_loggedInSubject.OnNext(true);
        }

        return true;
    }

    private void RaiseOnLoggedIn()
    {
        var handler = OnLoggedIn;

        if (handler is null)
        {
            return;
        }

        handler.Invoke(this, EventArgs.Empty);
    }
}