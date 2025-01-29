using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Errors;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Update.Services.EntityChecks.Checks;

internal sealed class CheckHasSameETag : PreUpdateCheckBase
{
    public override async Task<OutputState?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        var account = state.AccountToUpdate;
        var request = state.Request;

        if (account.Etag != request.ETag)
        {
            var problemDetails = ApiProblemDetailsFactory.CreateETagConflict(account, request.ETag);

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

