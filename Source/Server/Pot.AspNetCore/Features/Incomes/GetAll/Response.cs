using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Pagination;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Models;
using Pot.Data.Entities;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Incomes.GetAll;

internal sealed class Response : ResponseBase
{
    internal sealed class AccountModel
    {
        public required Guid RowId { get; init; }
    }

    [Description("A description of the income.")]
    public string Description { get; init; }

    [Description("When the income is next due.")]
    public DateOnly NextDue { get; init; }

    [Description("When the income is no longer a recurring source.")]
    public DateOnly? EndDate { get; init; }

    [Description("The income frequency type.")]
    public string Frequency { get; init; }

    [Description("The income frequency count.")]
    public int FrequencyCount { get; init; }

    [Description("The income amount.")]
    public double Amount { get; init; }

    [Description("The account this income is associated with.")]
    public AccountModel? Account { get; init; }

    public static Ok<PagedResponse<Response>> Ok(PageResult<IncomeEntity> incomes)
    {
        var results = incomes.Results.SelectToArray(income => new Response(income));

        var response = PagedResponse<Response>.CreateFromPageResult(incomes, results);

        return TypedResults.Ok(response);
    }

    private Response(IncomeEntity income)
    {
        _ = income.WhenNotNull();

        RowId = income.RowId;
        ETag = income.Etag;
        Description = income.Description;
        NextDue = income.NextDue;
        EndDate = income.EndDate;

        // Minimal APIs doesn't support Controller style ModelBinderProviders so
        // we can't use Frequency on this response.
        Frequency = income.Frequency.Name;

        FrequencyCount = income.FrequencyCount;
        Amount = income.Amount;

        if (income.Account is not null)
        {
            Account = new AccountModel
            {
                RowId = income.Account.RowId
            };
        }
    }
}
