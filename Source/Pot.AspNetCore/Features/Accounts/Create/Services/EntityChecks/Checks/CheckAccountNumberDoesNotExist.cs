using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Errors;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Create.Services.EntityChecks.Checks;

internal sealed class CheckAccountNumberDoesNotExist : PreCreateCheckBase
{
    public override async Task<OutputState?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        var account = state.AccountToCreate;

        var accountExists = await state.AccountRepository
            .AccountExistsAsync(account.Bsb, account.Number, cancellationToken)
            .ConfigureAwait(false);

        if (accountExists)
        {
            var problemDetails = ProblemDetailsFactory.CreateEntityExistsConflict(
                account,
                $"{nameof(AccountEntity.Bsb)}, {nameof(AccountEntity.Number)}",
                $"{account.Bsb}, {account.Number}");

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

