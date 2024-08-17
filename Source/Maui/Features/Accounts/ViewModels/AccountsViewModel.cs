using Pot.Maui.Domain.Accounts.Models;
using Pot.Maui.Domain.Accounts.Repository;
using Pot.Maui.Helpers;
using Pot.Maui.Mvvm;
using System.Reactive.Disposables;

namespace Pot.Maui.Features.Accounts.ViewModels;

public partial class AccountsViewModel : ViewModelBase
{
    private readonly IAccountRepository _accountRepository;

    public ObservableRangeCollection<Account> Accounts { get; } = [];

    public AccountsViewModel(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;

        Title = "Accounts";
    }

    public override async void OnActivate(CompositeDisposable disposables)
    {
        base.OnActivate(disposables);

        using (StartIsBusyScope())
        {
            var allAccounts = await _accountRepository.GetAllAccountsAsync();

            Accounts.ReplaceRange(allAccounts);
        }
    }
}
