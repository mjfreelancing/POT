using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.EntityFrameworkCore;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Errors;
using Pot.Data.Entities;
using Pot.Data.Specifications;

namespace Pot.AspNetCore.Features.Incomes.Create.Services.EntityChecks.Checks;

internal sealed class CheckDescriptionDoesNotExist : PreCreateCheckBase
{
    public override async Task<OutputState?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        state.Logger.LogCall(this);

        var income = state.IncomeToCreate;

        var predicate = IncomeSpecifications.IsSameDescription(state.IncomeToCreate.Account.Id, income.Description).Expression;

        var descriptionExists = await state.IncomeRepository
            .Where(predicate)
            .AnyAsync(income => income.Id != income.Id, cancellationToken)
            .ConfigureAwait(false);

        if (descriptionExists)
        {
            var problemDetails = ApiProblemDetailsFactory.CreateEntityExistsConflict(
                income,
                nameof(IncomeEntity.Description),
                income.Description);

            state.Logger.LogErrors(problemDetails);

            var accountError = new ServiceError(problemDetails);

            return new OutputState
            {
                FailResult = EnrichedResult.Fail<IncomeEntity>(accountError)
            };
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
