using Pot.App.Features.Accounts.GetAll.Models;
using Pot.Data.Repositories.Accounts.Dtos;

namespace Pot.App.Features.Accounts.GetAll.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this GetAccountDto dto)
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
            Allocated = account.Allocated,
            DailyAccrual = account.DailyAccrual,
            LinkedExpenses = dto.LinkedExpenses,
            LinkedIncomes = dto.LinkedIncomes
        };
    }
}