using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Models;
using Pot.Data.Entities;
using Pot.Data.Models;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.Get;

internal sealed class Response : ResponseBase
{
    internal sealed class AccountModel
    {
        public required Guid RowId { get; init; }
        public required string Description { get; init; }
    }

    [Description("A description of the income.")]
    public string Description { get; init; }

    [Description("When the income is next due.")]
    public DateOnly NextDue { get; init; }

    [Description("When the income is no longer a recurring source.")]
    public DateOnly? EndDate { get; init; }

    [Description("The income frequency type.")]
    public Frequency Frequency { get; init; }   // Serialized via EnrichedEnumJsonConverter<Frequency>

    [Description("The income frequency count.")]
    public int FrequencyCount { get; init; }

    [Description("The income amount.")]
    public double Amount { get; init; }

    [Description("The account this income is associated with.")]
    public AccountModel? Account { get; init; }

    public static Ok<Response> Ok(IncomeEntity income)
    {
        return TypedResults.Ok(new Response(income));
    }

    private Response(IncomeEntity income)
    {
        _ = income.WhenNotNull();

        RowId = income.RowId;
        ETag = income.Etag;
        Description = income.Description;
        NextDue = income.NextDue;
        EndDate = income.EndDate;
        Frequency = income.Frequency;
        FrequencyCount = income.FrequencyCount;
        Amount = income.Amount;

        var account = income.Account;

        if (account is not null)
        {
            Account = new AccountModel
            {
                RowId = account.RowId,
                Description = account.Description
            };
        }
    }
}
