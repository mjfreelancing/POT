using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.EntityFrameworkCore;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Errors;
using Pot.AspNetCore.Extensions;
using Pot.Data.Entities;
using Pot.Data.Specifications;

namespace Pot.AspNetCore.Features.Incomes.Update.Services.EntityChecks.Checks;

internal sealed class CheckDescriptionDoesNotExist : PreUpdateCheckBase
{
    public override async Task<OutputState?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        state.Logger.LogCall(this);

        var income = state.IncomeToUpdate;

        var predicate = IncomeSpecifications.IsSameDescription(state.IncomeAccount.Id, income.Description).Expression;

        var descriptionExists = await state.IncomeRepository
            .Where(predicate)
            .AnyAsync(entity => entity.Id != income.Id, cancellationToken)
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
