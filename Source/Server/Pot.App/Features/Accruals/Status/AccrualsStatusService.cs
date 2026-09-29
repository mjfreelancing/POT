using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.Extensions.Logging;
using Pot.App.Features.Accruals.Status.Models;
using Pot.Data.Repositories.Expenses;
using Pot.Data.Repositories.Incomes;

namespace Pot.App.Features.Accruals.Status;

/// <summary>
/// Default implementation of <see cref="IAccrualsStatusService"/>.
/// </summary>
internal sealed class AccrualsStatusService : IAccrualsStatusService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IIncomeRepository _incomeRepository;
    private readonly ILogger _logger;

    public AccrualsStatusService(IExpenseRepository expenseRepository, IIncomeRepository incomeRepository,
        ILogger<AccrualsStatusService> logger)
    {
        _expenseRepository = expenseRepository.WhenNotNull();
        _incomeRepository = incomeRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public async Task<EnrichedResult<Output>> GetStatusAsync(Input input, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        // Can't perform these in parallel without using a DbContextFactory to create separate DbContexts - TODO
        var expenseRenewalsRequired = await _expenseRepository
            .GetRequiredRenewalsAsync(input.AccountRowIds, input.AsOfDate, cancellationToken)
            .ConfigureAwait(false);

        var incomeRenewalsRequired = await _incomeRepository
            .GetRequiredRenewalsAsync(input.AccountRowIds, input.AsOfDate, cancellationToken)
            .ConfigureAwait(false);

        var output = new Output
        {
            ExpenseRenewalsRequired = expenseRenewalsRequired,
            IncomeRenewalsRequired = incomeRenewalsRequired
        };

        return EnrichedResult.Success(output);
    }
}
