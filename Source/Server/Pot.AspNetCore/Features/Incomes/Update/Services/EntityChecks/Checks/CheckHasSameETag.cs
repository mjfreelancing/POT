using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Errors;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Incomes.Update.Services.EntityChecks.Checks;

internal sealed class CheckHasSameETag : PreUpdateCheckBase
{
    public override async Task<OutputState?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        var request = state.Request;
        var income = state.IncomeToUpdate;

        if (income.Etag != request.ETag)
        {
            var problemDetails = ApiProblemDetailsFactory.CreateETagConflict(income, request.ETag);

            state.Logger.LogErrors(problemDetails);

            var incomeError = new ServiceError(problemDetails);

            return new OutputState
            {
                FailResult = EnrichedResult.Fail<IncomeEntity>(incomeError)
            };
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}

