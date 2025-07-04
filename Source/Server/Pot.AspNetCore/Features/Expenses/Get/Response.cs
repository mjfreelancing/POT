using AllOverIt.Assertion;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Expenses.Get.Models;
using Pot.AspNetCore.Models;
using Pot.Shared;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Get;

internal sealed class Response : ResponseBase
{
    internal sealed class AccountModel
    {
        public required Guid RowId { get; init; }
        public required string Description { get; init; }
    }

    [Description("A description of the Expense.")]
    public string Description { get; init; }

    [Description("When automatic allocations will begin accruing for this expense.")]
    public DateOnly AccrualStart { get; init; }

    [Description("When the Expense is next due.")]
    public DateOnly NextDue { get; init; }

    [Description("When the Expense is no longer recurring.")]
    public DateOnly? EndDate { get; init; }

    [Description("The Expense frequency type.")]
    public Frequency Frequency { get; init; }   // Serialized via EnrichedEnumJsonConverter<Frequency>

    [Description("The Expense frequency count.")]
    public int FrequencyCount { get; init; }

    [Description("The Expense amount.")]
    public double Amount { get; init; }

    [Description("The amount accrued for this expense.")]
    public double Accrued { get; init; }

    [Description("The account this Expense is associated with.")]
    public AccountModel? Account { get; init; }

    public static Ok<Response> Ok(Output expense)
    {
        return TypedResults.Ok(new Response(expense));
    }

    private Response(Output expense)
    {
        _ = expense.WhenNotNull();

        RowId = expense.RowId;
        Etag = expense.Etag;
        Description = expense.Description;
        NextDue = expense.NextDue;
        AccrualStart = expense.AccrualStart;
        EndDate = expense.EndDate;
        Frequency = expense.Frequency;
        FrequencyCount = expense.FrequencyCount;
        Amount = expense.Amount;
        Accrued = expense.Accrued;

        var account = expense.Account;

        Account = new AccountModel
        {
            RowId = account.RowId,
            Description = account.Description
        };
    }
}
