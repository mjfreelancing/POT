using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;

namespace Pot.App.Features.Incomes.Update.EntityChecks.Checks;

internal sealed class CheckHasSameEtag : PreUpdateCheckBase
{
    private readonly ILogger _logger;

    public CheckHasSameEtag(ILogger<CheckHasSameEtag> logger)
    {
        _logger = logger.WhenNotNull();
    }

    public override async Task<ProblemDetailsError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var input = state.Input;
        var incomeToUpdate = state.IncomeToUpdate;

        if (incomeToUpdate.Etag != input.Etag)
        {
            return ProblemDetailsErrorFactory.CreateEtagConflict("Income", input.Etag);
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}

