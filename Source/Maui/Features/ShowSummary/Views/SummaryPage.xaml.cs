using Pot.Maui.Features.ShowSummary.ViewModels;

namespace Pot.Maui.Features.ShowSummary.Views;

public partial class SummaryPage : ContentPage
{
    private readonly ISummaryViewModel _summaryViewModel;

    public SummaryPage(ISummaryViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

        _summaryViewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // TODO: error handle here or the view model

        // Updates AccountSummary and ExpenseSummary on the view model
        await _summaryViewModel.RefreshSummariesAsync();
    }
}