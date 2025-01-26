using AllOverIt.Patterns.Result;
using Microsoft.EntityFrameworkCore;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Errors;
using Pot.Data.Entities;
using Pot.Data.Specifications;

namespace Pot.AspNetCore.Features.Accounts.Create.Services.EntityChecks.Checks;

internal sealed class CheckDescriptionDoesNotExist : PreCreateCheckBase
{
    public override async Task<OutputState?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        var account = state.AccountToCreate;

        var predicate = AccountSpecifications.IsSameDescription(account.Description).Expression;

        var descriptionExists = await state.AccountRepository.Query()
            .AnyAsync(predicate, cancellationToken)
            .ConfigureAwait(false);

        if (descriptionExists)
        {
            var problemDetails = ProblemDetailsFactory.CreateEntityExistsConflict(
                account,
                nameof(AccountEntity.Description),
                account.Description);

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
