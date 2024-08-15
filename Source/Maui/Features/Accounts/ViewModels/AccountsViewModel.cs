using Pot.Maui.Domain.Accounts.Models;
using Pot.Maui.Domain.Accounts.Repository;
using Pot.Maui.Helpers;

namespace Pot.Maui.Features.Accounts.ViewModels;

//public interface IAccountsViewModel : IViewModel
//{
//    ObservableRangeCollection<Account> Accounts { get; }

//    Task RefreshAsync();
//}

public partial class AccountsViewModel : ViewModelBase//, IAccountsViewModel
{
    private readonly IAccountRepository _accountRepository;

    public ObservableRangeCollection<Account> Accounts { get; } = [];

    public AccountsViewModel(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;

        Title = "Accounts";
    }

    public async Task RefreshAsync()
    {
        var allAccounts = await _accountRepository.GetAllAccountsAsync();

        Accounts.ReplaceRange(allAccounts);
    }
}
