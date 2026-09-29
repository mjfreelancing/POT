using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;

namespace Pot.App.Features.Expenses.Update.EntityChecks.Checks;

/// <summary>
/// Rejects an update when the expense has changed since the client last read it, as reported by a mismatched
/// concurrency token.
/// </summary>
internal sealed class CheckHasSameEtag : PreUpdateCheckBase
{
    private readonly ILogger _logger;

    public CheckHasSameEtag(ILogger<CheckHasSameEtag> logger)
    {
        _logger = logger.WhenNotNull();
    }

    /// <summary>
    /// Fails the pipeline when the persisted concurrency token does not match the one supplied by the client.
    /// </summary>
    /// <param name="state">The state carried through the pre-update check pipeline.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <see cref="ApiDetailError"/> when the tokens differ; otherwise the result of the next check in the chain.
    /// </returns>
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var input = state.Input;
        var expenseToUpdate = state.ExpenseToUpdate;

        if (expenseToUpdate.Etag != input.Etag)
        {
            return ApiDetailErrorFactory.CreateEtagConflict("Expense", input.Etag);
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}

