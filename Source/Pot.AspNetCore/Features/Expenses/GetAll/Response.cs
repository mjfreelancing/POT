using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Models;
using Pot.Data.Entities;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.GetAll;

internal sealed class Response : ResponseBase
{
    [Description("A description of the expense.")]
    public string Description { get; init; }

    [Description("When the expense is next due.")]
    public DateOnly NextDue { get; init; }

    [Description("When automatic allocations will begin accruing for this expense.")]
    public DateOnly AccrualStart { get; init; }

    [Description("The expense frequency type.")]
    public string Frequency { get; init; }

    [Description("The expense frequency count.")]
    public int FrequencyCount { get; init; }

    [Description("Indicates if the expense is recurring.")]
    public bool Recurring { get; init; }

    [Description("The expense amount.")]
    public double Amount { get; init; }

    [Description("The amount allocated towards this expense.")]
    public double Allocated { get; init; }

    public static Ok<Response[]> Ok(List<ExpenseEntity> expenses)
    {
        var responses = expenses.SelectToArray(expense => new Response(expense));

        return TypedResults.Ok(responses);
    }

    private Response(ExpenseEntity account)
    {
        _ = account.WhenNotNull();

        RowId = account.RowId;
        ETag = account.Etag;
        Description = account.Description;
        NextDue = account.NextDue;
        AccrualStart = account.AccrualStart;

        // Minimal APIs doesn't support Controller style ModelBinderProviders so
        // we can't use ExpenseFrequency on this response.
        Frequency = account.Frequency.Name;

        FrequencyCount = account.FrequencyCount;
        Recurring = account.Recurring;
        Amount = account.Amount;
        Allocated = account.Allocated;
    }
}
