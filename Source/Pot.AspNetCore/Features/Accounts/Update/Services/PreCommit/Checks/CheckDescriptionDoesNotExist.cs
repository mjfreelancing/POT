using AllOverIt.Patterns.Result;
using AllOverIt.Patterns.Specification.Extensions;
using Microsoft.EntityFrameworkCore;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Errors;
using Pot.Data.Entities;
using Pot.Data.Specifications;

namespace Pot.AspNetCore.Features.Accounts.Update.Services.PreCommit.Checks;

internal sealed class CheckDescriptionDoesNotExist : PreUpdateCheckBase
{
    public override async Task<OutputState?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        var account = state.AccountToUpdate;
        var request = state.Request;

        if (account.Description != request.Description)
        {
            var notSameAccount = AccountSpecifications.IsSameBsbNumber(request.Bsb, request.Number).Not();
            var sameDescription = AccountSpecifications.IsSameDescription(request.Description);
            var predicate = notSameAccount.And(sameDescription).Expression;

            var descriptionExists = await state.AccountRepository.Query()
                .AnyAsync(predicate, cancellationToken)
                .ConfigureAwait(false);

            if (descriptionExists)
            {
                var problemDetails = ProblemDetailsFactory.CreateEntityExistsConflict(
                    account,
                    nameof(AccountEntity.Description),
                    request.Description);

                state.Logger.LogErrors(problemDetails);

                var accountError = new ServiceError(problemDetails);

                return new OutputState
                {
                    FailResult = EnrichedResult.Fail<AccountEntity>(accountError)
                };
            }
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
