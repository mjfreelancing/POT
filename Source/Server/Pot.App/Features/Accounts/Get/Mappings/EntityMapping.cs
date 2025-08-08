using Pot.App.Features.Accounts.Get.Models;
using Pot.Data.Repositories.Accounts.Dtos;

namespace Pot.App.Features.Accounts.Get.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this GetAccountDto dto)
    {
        var account = dto.Account;

        return new Output
        {
            RowId = account.RowId,
            Etag = account.Etag,
            ExcludeFromCalcs = account.ExcludeFromCalcs,
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