using Pot.Maui.Features.Login.ViewModels;
using Pot.Maui.Features.ShowSummary.Views;

namespace Pot.Maui.Features.Login.Views;

public partial class LoginPage : ContentPage
{
    //private CompositeDisposable? _disposables;
    private readonly ILoginViewModel _loginViewModel;

    public LoginPage(ILoginViewModel loginViewModel)
    {
        InitializeComponent();

        BindingContext = loginViewModel;

        _loginViewModel = loginViewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _loginViewModel.OnLoggedIn += HandleOnLoggedIn;

        //_disposables = [];

        //if (_loginViewModel.IsLoggedIn)
        //{
        //    await Shell.Current.GoToAsync($"//{nameof(SummaryPage)}");
        //}
        //else
        //{
        //    //
        //}

        //var loggedin = true;
        //if(loggedin)
        //await Shell.Current.GoToAsync($"//{nameof(CoffeeEquipmentPage)}");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _loginViewModel.OnLoggedIn += HandleOnLoggedIn;

        //_disposables?.Dispose();
        //_disposables = null;
    }

    private async void HandleOnLoggedIn(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(SummaryPage)}");
    }

    private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        // Add a registration page
        await Shell.Current.DisplayAlert("Registration", "Add a registration page", "OK");

        //await Shell.Current.GoToAsync($"{nameof(RegistrationPage)}");
    }
}