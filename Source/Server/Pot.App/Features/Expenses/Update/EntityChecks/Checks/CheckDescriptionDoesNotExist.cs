using AllOverIt.Assertion;
using AllOverIt.Expressions;
using AllOverIt.Logging.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Expenses;
using Pot.Data.Specifications;

namespace Pot.App.Features.Expenses.Update.EntityChecks.Checks;

/// <summary>
/// Rejects an update when the expense description already exists for another expense in the same account.
/// </summary>
internal sealed class CheckDescriptionDoesNotExist : PreUpdateCheckBase
{

    private readonly IExpenseRepository _expenseRepository;
    private readonly ILogger _logger;

    public CheckDescriptionDoesNotExist(IExpenseRepository expenseRepository, ILogger<CheckDescriptionDoesNotExist> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <summary>
    /// Fails the pipeline when another expense in the same account already uses the supplied description.
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

        var predicate = ExpenseSpecifications
            .IsSameDescription(state.ExpenseAccount.Id, input.Description).Expression
            .And(entity => entity.Id != state.ExpenseToUpdate.Id);

        var descriptionExists = await _expenseRepository.Expenses
            .AnyAsync(predicate, cancellationToken)
            .ConfigureAwait(false);

        if (descriptionExists)
        {
            return ApiDetailErrorFactory.CreateEntityExistsError(
                nameof(ExpenseEntity.Description),
                input.Description,
                "The expense description already exists");
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
