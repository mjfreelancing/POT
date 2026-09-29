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

/// <summary>
/// Rejects an update when the income description already exists for another income in the same account.
/// </summary>
internal sealed class CheckDescriptionDoesNotExist : PreUpdateCheckBase
{

    private readonly IIncomeRepository _incomeRepository;
    private readonly ILogger _logger;

    public CheckDescriptionDoesNotExist(IIncomeRepository incomeRepository, ILogger<CheckDescriptionDoesNotExist> logger)
    {
        _incomeRepository = incomeRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <summary>
    /// Fails the pipeline when another income in the same account already uses the supplied description.
    /// </summary>
    /// <param name="state">The state carried through the pre-update check pipeline.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <see cref="ApiDetailError"/> when the description already exists; otherwise the result of the next check
    /// in the chain.
    /// </returns>
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
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
            return ApiDetailErrorFactory.CreateEntityExistsError(
                nameof(IncomeEntity.Description),
                input.Description,
                "The income description already exists");
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
