using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Errors;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Incomes.Create.Services.EntityChecks.Checks;

internal sealed class CheckAccountExistsWhenProvided : PreCreateCheckBase
{
    public override async Task<OutputState?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        state.Logger.LogCall(this);

        if (state.AccountRowId.HasValue)
        {
            state.IncomeToCreate.Account = await state.AccountRepository
                .GetAccountOrDefaultAsync(state.AccountRowId.Value, cancellationToken)
                .ConfigureAwait(false);

            if (state.IncomeToCreate.Account is null)
            {
                var problemDetails = ApiProblemDetailsFactory.CreateUnprocessableEntity(
                    ErrorCodes.NotFound,
                    nameof(Request.AccountRowId),
                    state.AccountRowId.Value,
                    "The account does not exist");

                state.Logger.LogErrors(problemDetails);

                var accountError = new ServiceError(problemDetails);

                return new OutputState
                {
                    FailResult = EnrichedResult.Fail<IncomeEntity>(accountError)
                };
            }
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}

