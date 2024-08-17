using Pot.Maui.Features.Summary.ViewModels;
using Pot.Maui.Mvvm;

namespace Pot.Maui.Features.Summary.Views;

public partial class SummaryPage : ViewModelContentPage<SummaryViewModel>
{
    public SummaryPage(SummaryViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}