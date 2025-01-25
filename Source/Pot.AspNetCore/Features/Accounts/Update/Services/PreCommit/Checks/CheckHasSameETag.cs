using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Errors;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Update.Services.PreCommit.Checks;

internal sealed class CheckHasSameETag : PreCommitCheckBase
{
    public override async Task<OutputState?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        var account = state.AccountToUpdate;
        var request = state.Request;

        if (account.Etag != request.ETag)
        {
            var problemDetails = ProblemDetailsFactory.CreateETagConflict(account);

            state.Logger.LogErrors(problemDetails);

            var accountError = new ServiceError(problemDetails);

            return new OutputState
            {
                FailResult = EnrichedResult.Fail<AccountEntity>(accountError)
            };
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}

