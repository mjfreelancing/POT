using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Errors;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Accounts.Update.Services.EntityChecks.Checks;

internal sealed class CheckAccountNumberDoesNotExist : PreUpdateCheckBase
{
    public override async Task<OutputState?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        var account = state.AccountToUpdate;
        var request = state.Request;

        var differentAccountNumber = !(account.Bsb == request.Bsb && account.Number == request.Number);

        if (differentAccountNumber)
        {
            var accountExists = await state.AccountRepository
                .AccountExistsAsync(account.Bsb, account.Number, cancellationToken)
                .ConfigureAwait(false);

            if (accountExists)
            {
                var problemDetails = ApiProblemDetailsFactory.CreateEntityExistsConflict(
                    account,
                    $"{nameof(AccountEntity.Bsb)}, {nameof(AccountEntity.Number)}",
                    $"{request.Bsb}, {request.Number}");

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
