using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.AspNetCore.Models;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;

namespace Pot.AspNetCore.Features.Expenses.GetAll;

using OkResult = Ok<PagedResponse<Response>>;

internal sealed class Handler
{
    public static async Task<Results<OkResult, NotFound, ProblemHttpResult>> Invoke(
        /*[Description("...")]*/ Request request,
        IAccountRepository accountRepository,
        IExpenseRepository expenseRepository,
        ILogger<Handler> logger,
        CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var account = await accountRepository.GetAccountOrDefaultAsync(request.AccountId, cancellationToken);

        if (account is null)
        {
            return TypedResults.NotFound();
        }

        var expenses = await expenseRepository.GetExpensesForAccountAsync(request.AccountId, request.Paging, cancellationToken);

        return Response.Ok(expenses);
    }
}
