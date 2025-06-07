using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Pagination;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Models;
using Pot.Data.Entities;
using Pot.Shared;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.GetAll;

internal sealed class Response : ResponseBase
{
    [Description("A description of the expense.")]
    public string Description { get; init; }

    [Description("When the expense is next due.")]
    public DateOnly NextDue { get; init; }

    [Description("When the expense is no longer a recurring concern.")]
    public DateOnly? EndDate { get; init; }

    [Description("When automatic allocations will begin accruing for this expense.")]
    public DateOnly AccrualStart { get; init; }

    [Description("The expense frequency type.")]
    public Frequency Frequency { get; init; }

    [Description("The expense frequency count.")]
    public int FrequencyCount { get; init; }

    [Description("Indicates if the expense is recurring.")]
    public bool Recurring { get; init; }

    [Description("The expense amount.")]
    public double Amount { get; init; }

    [Description("The amount allocated towards this expense.")]
    public double Allocated { get; init; }

    public static Ok<PagedResponse<Response>> Ok(PageResult<ExpenseEntity> expenses)
    {
        var results = expenses.Results.SelectToArray(expense => new Response(expense));

        var response = PagedResponse<Response>.CreateFromPageResult(expenses, results);

        return TypedResults.Ok(response);
    }

    private Response(ExpenseEntity expense)
    {
        _ = expense.WhenNotNull();

        RowId = expense.RowId;
        Etag = expense.Etag;
        Description = expense.Description;
        NextDue = expense.NextDue;
        EndDate = expense.EndDate;
        AccrualStart = expense.AccrualStart;
        Frequency = expense.Frequency;
        FrequencyCount = expense.FrequencyCount;
        Amount = expense.Amount;
        Allocated = expense.Allocated;
    }
}
