using Pot.Maui.Features.Summary.ViewModels;

namespace Pot.Maui.Features.Summary.Views;

public partial class SummaryPage : ContentPage
{
    private readonly SummaryViewModel _summaryViewModel;

    public SummaryPage(SummaryViewModel viewModel)
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