using AllOverIt.Assertion;
using AllOverIt.Expressions;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Incomes;
using Pot.Data.Specifications;

namespace Pot.App.Features.Incomes.Update.EntityChecks.Checks;

internal sealed class CheckDescriptionDoesNotExist : PreUpdateCheckBase
{

    private readonly IIncomeRepository _incomeRepository;
    private readonly ILogger _logger;

    public CheckDescriptionDoesNotExist(IIncomeRepository incomeRepository, ILogger<CheckDescriptionDoesNotExist> logger)
    {
        _incomeRepository = incomeRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public override async Task<ProblemDetailsError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var input = state.Input;

        var predicate = IncomeSpecifications
            .IsSameDescription(state.IncomeAccount.Id, input.Description).Expression
            .And(entity => entity.Id != state.IncomeToUpdate.Id);

        var descriptionExists = await _incomeRepository.Incomes
            .AnyAsync(predicate, cancellationToken)
            .ConfigureAwait(false);

        if (descriptionExists)
        {
            return ProblemDetailsErrorFactory.CreateEntityExistsError(
                "Income",
                 nameof(IncomeEntity.Description),
                input.Description);
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
