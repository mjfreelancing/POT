using CommunityToolkit.Mvvm.Input;

namespace Pot.Maui.Features.Login.ViewModels;

public interface ILoginViewModel : IViewModel
{
    event EventHandler? OnLoggedIn;

    //bool IsLoggedIn { get; }

    //IObservable<bool> LoggedInStatusChanged { get; }

    IAsyncRelayCommand LoginCommand { get; }
}
