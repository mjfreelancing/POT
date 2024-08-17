using CommunityToolkit.Mvvm.Input;

namespace Pot.Maui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            BindingContext = this;
        }

        [RelayCommand]
        private static void ShowAppInfo()
        {
            AppInfo.ShowSettingsUI();
        }
    }
}
