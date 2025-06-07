using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Errors;
using Pot.AspNetCore.Extensions;
using Pot.Data.Entities;

namespace Pot.AspNetCore.Features.Incomes.Update.Services.EntityChecks.Checks;

internal sealed class CheckHasSameEtag : PreUpdateCheckBase
{
    public override async Task<OutputState?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        var request = state.Request;
        var income = state.IncomeToUpdate;

        if (income.Etag != request.Etag)
        {
            var problemDetails = ApiProblemDetailsFactory.CreateEtagConflict(income, request.Etag);

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

