using Pot.App.Features.Incomes.Get.Models;
using Pot.Data.Entities;

namespace Pot.App.Features.Incomes.Get.Mappings;

internal static class EntityMapping
{
    public static Output MapToOutput(this IncomeEntity income)
    {
        return new Output
        {
            RowId = income.RowId,
            Etag = income.Etag,
            ExcludeFromCalcs = income.ExcludeFromCalcs,
            Description = income.Description,
            NextDue = income.NextDue,
            EndDate = income.EndDate,
            Frequency = income.Frequency,
            FrequencyCount = income.FrequencyCount,
            Amount = income.Amount,
            Note = income.Note,
            Account = new Output.AccountModel
            {
                RowId = income.Account.RowId,
                Description = income.Account.Description
            }
        };
    }
}