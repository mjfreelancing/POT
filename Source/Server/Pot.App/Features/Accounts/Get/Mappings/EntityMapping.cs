using Pot.App.Features.Accounts.Get.Models;
using Pot.Data.Repositories.Accounts.Dtos;
using Pot.Shared.Models;

namespace Pot.App.Features.Accounts.Get.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this AccountWithLinkedCounts accountDetails, AccountAccrualView accrualView)
    {
        var account = accountDetails.Account;

        return new Output
        {
            RowId = account.RowId,
            Etag = account.Etag,
            Description = account.Description,
            Balance = account.Balance,
            Reserved = account.Reserved,
            TotalExpenseAccrued = accrualView.TotalExpenseAccrued,
            TotalArrears = accrualView.TotalArrears,
            TotalCommitted = accrualView.TotalCommitted,
            StableExpenseAccrual = accrualView.StableExpenseAccrual,
            LinkedExpenses = accountDetails.LinkedExpenses,
            LinkedIncomes = accountDetails.LinkedIncomes
        };
    }
}