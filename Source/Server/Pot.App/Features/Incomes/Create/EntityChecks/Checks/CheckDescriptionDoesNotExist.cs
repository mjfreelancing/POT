using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Incomes;
using Pot.Data.Specifications;

namespace Pot.App.Features.Incomes.Create.EntityChecks.Checks;

internal sealed class CheckDescriptionDoesNotExist : PreCreateCheckBase
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

        var incomeToCreate = state.IncomeToCreate;

        var predicate = IncomeSpecifications.IsSameDescription(state.IncomeToCreate.Account.Id, incomeToCreate.Description).Expression;

        var descriptionExists = await _incomeRepository.Incomes
            .AnyAsync(predicate, cancellationToken)
            .ConfigureAwait(false);

        if (descriptionExists)
        {
            if (descriptionExists)
            {
                return ProblemDetailsErrorFactory.CreateEntityExistsError(
                    "Income",
                    nameof(IncomeEntity.Description),
                    incomeToCreate.Description);
            }

            return await base.HandleAsync(state, cancellationToken);
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
