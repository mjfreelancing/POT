using AllOverIt.Extensions;
using AllOverIt.Pagination;
using System.ComponentModel;

namespace Pot.AspNetCore.Models;

public class PagedResponse<TResult>
{
    [Description("The page of results.")]
    public required TResult[] Results { get; init; }

    [Description("The total number of results available.")]
    public required int TotalCount { get; init; }

    [Description("The continuation token for the current page.")]
    public required string? CurrentToken { get; init; }

    [Description("The continuation token for the previous page.")]
    public required string? PreviousToken { get; init; }

    [Description("The continuation token for the next page.")]
    public required string? NextToken { get; init; }

    public static PagedResponse<TResult> CreateFromPageResult<TFirst>(PageResult<TFirst> firstResults, IEnumerable<TResult> results)
    {
        return new PagedResponse<TResult>
        {
            Results = results.AsArray(),
            TotalCount = firstResults.TotalCount,
            CurrentToken = firstResults.CurrentToken,
            PreviousToken = firstResults.PreviousToken,
            NextToken = firstResults.NextToken
        };
    }
}