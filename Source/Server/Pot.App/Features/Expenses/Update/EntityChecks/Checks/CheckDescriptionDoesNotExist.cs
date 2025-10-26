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

internal sealed class CheckDescriptionDoesNotExist : PreUpdateCheckBase
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

        var input = state.Input;

        var predicate = ExpenseSpecifications
            .IsSameDescription(state.ExpenseAccount.Id, input.Description).Expression
            .And(entity => entity.Id != state.ExpenseToUpdate.Id);

        var descriptionExists = await _expenseRepository.Expenses
            .AnyAsync(predicate, cancellationToken)
            .ConfigureAwait(false);

        if (descriptionExists)
        {
            return ProblemDetailsErrorFactory.CreateEntityExistsError(
                "Expense",
                 nameof(ExpenseEntity.Description),
                input.Description);
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
