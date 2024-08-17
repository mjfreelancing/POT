namespace Pot.Maui;

public partial class App : Application
{
    public App()
    {
        //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NCaF5cXmZCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWXdfc3VcQ2lcU0ZyWUQ=");

        InitializeComponent();

        /*Application.Current.*/
        UserAppTheme = AppTheme.Dark;

        MainPage = new AppShell();
    }
}
