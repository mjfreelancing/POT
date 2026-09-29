using Pot.App.Features.Accounts.GetAll.Models;
using Pot.Data.Repositories.Accounts.Dtos;
using Pot.Shared.Models;

namespace Pot.App.Features.Accounts.GetAll.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this AccountWithLinkedCounts dto, AccountAccrualView accrual)
    {
        var account = dto.Account;

        return new Output
        {
            RowId = account.RowId,
            Etag = account.Etag,
            Description = account.Description,
            Balance = account.Balance,
            Reserved = account.Reserved,
            TotalExpenseAccrued = accrual.TotalExpenseAccrued,
            TotalArrears = accrual.TotalArrears,
            TotalCommitted = accrual.TotalCommitted,
            StableExpenseAccrual = accrual.StableExpenseAccrual,
            LinkedExpenses = dto.LinkedExpenses,
            LinkedIncomes = dto.LinkedIncomes
        };
    }
}