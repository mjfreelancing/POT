using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Expenses.GetAll.Models;
using Pot.AspNetCore.Models;
using Pot.Shared.Enumerations;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.GetAll;

internal sealed class Response : ResponseBase
{
    internal sealed class AccountModel
    {
        public required Guid RowId { get; init; }
        public required string Description { get; init; }
    }

    [Description("Indicates if the expense is excluded from calculations such as accruals")]
    public bool ExcludeFromCalcs { get; set; }

    [Description("A description of the expense")]
    public string Description { get; init; }

    [Description("When automatic allocations will begin accruing for this expense")]
    public DateOnly? AccrualStart { get; init; }

    [Description("When the expense is next due")]
    public DateOnly NextDue { get; init; }

    [Description("When the Expense is no longer recurring")]
    public DateOnly? EndDate { get; init; }

    [Description("The expense accrual policy")]
    public AccrualPolicy AccrualPolicy { get; init; }       // Serialized via EnrichedEnumJsonConverter<AccrualPolicy>

    [Description("The expense frequency type")]
    public Frequency Frequency { get; init; }               // Serialized via EnrichedEnumJsonConverter<AccrualPolicy>

    [Description("The expense frequency count")]
    public int FrequencyCount { get; init; }

    [Description("The expense amount")]
    public double Amount { get; init; }

    [Description("The amount accrued towards this expense")]
    public double Accrued { get; init; }

    [Description("The account this expense is associated with")]
    public AccountModel? Account { get; init; }

    [Description("A note about the expense")]
    public string? Note { get; init; }

    public static Ok<Response[]> Ok(List<Output> expenses)
    {
        var responses = expenses.SelectToArray(expense => new Response(expense));

        return TypedResults.Ok(responses);
    }

    private Response(Output expense)
    {
        _ = expense.WhenNotNull();

        RowId = expense.RowId;
        Etag = expense.Etag;
        ExcludeFromCalcs = expense.ExcludeFromCalcs;
        Description = expense.Description;
        NextDue = expense.NextDue;
        EndDate = expense.EndDate;
        AccrualStart = expense.AccrualStart;
        AccrualPolicy = expense.AccrualPolicy;
        Frequency = expense.Frequency;
        FrequencyCount = expense.FrequencyCount;
        Amount = expense.Amount;
        Accrued = expense.Accrued;
        Note = expense.Note;

        var account = expense.Account;

        Account = new AccountModel
        {
            RowId = account.RowId,
            Description = account.Description
        };
    }
}
