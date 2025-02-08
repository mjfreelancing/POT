using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Pot.Data.Repositories.Accounts;
using Pot.Data.Repositories.Expenses;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.GetAll;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response[]>, NotFound, ProblemHttpResult>> Invoke([Description("The account Id.")] Guid accountId,
        IAccountRepository accountRepository, IExpenseRepository expenseRepository, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var account = await accountRepository.GetAccountOrDefaultAsync(accountId, cancellationToken);

        if (account is null)
        {
            return TypedResults.NotFound();
        }

        var expenses = await expenseRepository
            .Where(expense => expense.Account != null && expense.Account.RowId == accountId)
            .ToListAsync(cancellationToken);

        return Response.Ok(expenses);
    }
}
