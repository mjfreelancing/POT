using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Incomes.Get.Models;
using Pot.AspNetCore.Models;
using Pot.Shared.Enumerations;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.Get;

internal sealed class Response : ResponseBase
{
    internal sealed class AccountModel
    {
        public required Guid RowId { get; init; }
        public required string Description { get; init; }
    }

    [Description("Indicates if the income is excluded from calculations")]
    public bool ExcludeFromCalcs { get; set; }

    [Description("A description of the income")]
    public string Description { get; init; }

    [Description("When the income is next due.")]
    public DateOnly NextDue { get; init; }

    [Description("When the income is no longer a recurring source")]
    public DateOnly? EndDate { get; init; }

    [Description("The income frequency type")]
    public Frequency Frequency { get; init; }   // Serialized via EnrichedEnumJsonConverter<Frequency>

    [Description("The income frequency count")]
    public int FrequencyCount { get; init; }

    [Description("The income amount")]
    public double Amount { get; init; }

    [Description("The account this income is associated with")]
    public AccountModel? Account { get; init; }

    [Description("A note about the income")]
    public string? Note { get; init; }

    public static Ok<Response> Ok(Output income)
    {
        return TypedResults.Ok(new Response(income));
    }

    private Response(Output income)
    {
        _ = income.WhenNotNull();

        RowId = income.RowId;
        Etag = income.Etag;
        ExcludeFromCalcs = income.ExcludeFromCalcs;
        Description = income.Description;
        NextDue = income.NextDue;
        EndDate = income.EndDate;
        Frequency = income.Frequency;
        FrequencyCount = income.FrequencyCount;
        Amount = income.Amount;
        Note = income.Note;

        var account = income.Account;

        Account = new AccountModel
        {
            RowId = account.RowId,
            Description = account.Description
        };
    }
}
