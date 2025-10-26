using Pot.App.Features.Accounts.GetAll.Models;
using Pot.Data.Repositories.Accounts.Dtos;

namespace Pot.App.Features.Accounts.GetAll.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this AccountWithLinkedCounts dto)
    {
        var account = dto.Account;

        return new Output
        {
            RowId = account.RowId,
            Etag = account.Etag,
            Bsb = account.Bsb,
            Number = account.Number,
            Description = account.Description,
            Balance = account.Balance,
            Reserved = account.Reserved,
            TotalExpenseAccrued = account.TotalExpenseAccrued,
            DailyExpenseAccrual = account.DailyExpenseAccrual,
            LinkedExpenses = dto.LinkedExpenses,
            LinkedIncomes = dto.LinkedIncomes
        };
    }
}