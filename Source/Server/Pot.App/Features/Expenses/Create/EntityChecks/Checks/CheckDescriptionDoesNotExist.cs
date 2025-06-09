using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Expenses;
using Pot.Data.Specifications;

namespace Pot.App.Features.Expenses.Create.EntityChecks.Checks;

internal sealed class CheckDescriptionDoesNotExist : PreCreateCheckBase
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ILogger _logger;

    public CheckDescriptionDoesNotExist(IExpenseRepository expenseRepository, ILogger<CheckDescriptionDoesNotExist> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public override async Task<ProblemDetailsError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var expenseToCreate = state.ExpenseToCreate;

        var predicate = ExpenseSpecifications.IsSameDescription(state.ExpenseToCreate.Account.Id, expenseToCreate.Description).Expression;

        var descriptionExists = await _expenseRepository
            .AnyAsync(predicate, cancellationToken)
            .ConfigureAwait(false);

        if (descriptionExists)
        {
            if (descriptionExists)
            {
                return ProblemDetailsErrorFactory.CreateEntityExistsError(
                    "Expense",
                    nameof(ExpenseEntity.Description),
                    expenseToCreate.Description);
            }

            return await base.HandleAsync(state, cancellationToken);
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
