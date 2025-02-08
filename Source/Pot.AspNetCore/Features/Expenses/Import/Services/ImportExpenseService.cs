using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.EntityFrameworkCore;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Concerns.Validation.Extensions;
using Pot.AspNetCore.Errors;
using Pot.AspNetCore.Features.Expenses.Import.Models;
using Pot.AspNetCore.Features.Expenses.Import.Validators;
using Pot.Data.Entities;
using Pot.Data.UnitOfWork;

namespace Pot.AspNetCore.Features.Expenses.Import.Services;

internal sealed class ImportExpenseService : IImportExpenseService
{
    private record ExpenseEntityInfo(ExpenseEntity ExpenseEntity, bool Created);

    private readonly IExpenseCsvRowValidator _csvRowValidator;
    private readonly IPotUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ImportExpenseService(IExpenseCsvRowValidator csvRowValidator, IPotUnitOfWork unitOfWork, ILogger<ImportExpenseService> logger)
    {
        _csvRowValidator = csvRowValidator.WhenNotNull();
        _unitOfWork = unitOfWork.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<ImportSummary>> ImportExpensesAsync(IEnumerable<ExpenseCsvRow> csvRows,
        CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_unitOfWork.WithTracking())
        {
            var problemDetailsErrors = new List<CsvProblemDetailsError>();

            var recordCount = 0;
            var imported = 0;
            var updated = 0;
            var row = 1;            // Skip the header row

            foreach (var csvRow in csvRows)
            {
                row++;
                recordCount++;

                // Only validating each row. Not looking for duplicates or other possible conflicts.
                CheckImportColumns(row, csvRow, problemDetailsErrors);

                // If we already have at least one error then only look for more errors.
                if (problemDetailsErrors.Count > 0)
                {
                    continue;
                }

                var expenseEntityInfo = await CreateOrUpdateExpenseAsync(csvRow, cancellationToken).ConfigureAwait(false);

                if (expenseEntityInfo.Created)
                {
                    imported++;
                }
                else
                {
                    updated++;
                }

                var expenseEntity = expenseEntityInfo.ExpenseEntity;
                var csvAccountId = csvRow.AccountId?.As<Guid?>();

                await UpdateExpenseAccountAsync(expenseEntity, csvAccountId, row, problemDetailsErrors, cancellationToken).ConfigureAwait(false);
            }

            if (problemDetailsErrors.Count > 0)
            {
                var problemDetails = ApiProblemDetailsFactory.CreateUnprocessableEntity(problemDetailsErrors);
                var serviceError = new ServiceError(problemDetails);

                return EnrichedResult.Fail<ImportSummary>(serviceError);
            }

            await _unitOfWork
                .SaveAsync(cancellationToken)
                .ConfigureAwait(false);

            var result = new ImportSummary
            {
                Imported = imported,
                Updated = updated,
                Total = recordCount
            };

            return EnrichedResult.Success(result);
        }
    }

    private void CheckImportColumns(int row, ExpenseCsvRow import, List<CsvProblemDetailsError> problemDetailsErrors)
    {
        var validationResult = _csvRowValidator.Validate(import);

        if (!validationResult.IsValid)
        {
            var errors = validationResult
                .ToProblemDetailsErrors()
                .Select(error => new CsvProblemDetailsError
                {
                    ImportRow = row,
                    PropertyName = error.PropertyName,
                    ErrorCode = error.ErrorCode,
                    AttemptedValue = error.AttemptedValue,
                    ErrorMessage = error.ErrorMessage
                });

            problemDetailsErrors.AddRange(errors);
        }
    }

    private async Task<ExpenseEntityInfo> CreateOrUpdateExpenseAsync(ExpenseCsvRow csvRow, CancellationToken cancellationToken)
    {
        ExpenseEntity? expenseEntity = null;

        var csvExpenseId = csvRow.ExpenseId?.As<Guid?>();

        if (csvExpenseId.HasValue)
        {
            // Will not exist if re-importing a file to recover data
            expenseEntity = await _unitOfWork.ExpenseRepository
               .Where(expense => expense.RowId == csvExpenseId)
               .Include(expense => expense.Account)
               .SingleOrDefaultAsync(cancellationToken)
               .ConfigureAwait(false);
        }

        if (expenseEntity is null)
        {
            expenseEntity = CreateExpenseEntity(csvExpenseId, csvRow);
            return new ExpenseEntityInfo(expenseEntity, true);
        }

        UpdateExistingExpense(expenseEntity, csvRow);

        return new ExpenseEntityInfo(expenseEntity, false);
    }

    private async Task UpdateExpenseAccountAsync(ExpenseEntity expenseEntity, Guid? csvAccountId, int row,
        List<CsvProblemDetailsError> problemDetailsErrors, CancellationToken cancellationToken)
    {
        // If the expense has an account but it doesn't match the provided Account Id,
        // then move the expense to the new account (or unassign if not provided)
        if (expenseEntity.Account is not null && expenseEntity.Account.RowId != csvAccountId)
        {
            expenseEntity.Account.Expenses.Remove(expenseEntity);
            expenseEntity.Account = null;
        }

        if (csvAccountId.HasValue && expenseEntity.Account is null)
        {
            var targetAccountEntity = await _unitOfWork.AccountRepository
                .GetAccountOrDefaultAsync(csvAccountId.Value, cancellationToken)
                .ConfigureAwait(false);

            if (targetAccountEntity is null)
            {
                var errorDetails = new CsvProblemDetailsError
                {
                    ImportRow = row,
                    PropertyName = nameof(ExpenseCsvRow.AccountId),
                    ErrorCode = ErrorCodes.NotFound,
                    AttemptedValue = csvAccountId!.Value,
                    ErrorMessage = "The Account does not exist"
                };

                problemDetailsErrors.Add(errorDetails);
                return;
            }

            targetAccountEntity.Expenses.Add(expenseEntity);
        }
    }

    private ExpenseEntity CreateExpenseEntity(Guid? expenseId, ExpenseCsvRow import)
    {
        var expenseEntity = new ExpenseEntity
        {
            RowId = expenseId ?? Guid.NewGuid(),
            Description = import.Description,
            NextDue = import.NextDue,
            AccrualStart = import.AccrualStart,
            Frequency = import.Frequency,
            FrequencyCount = import.FrequencyCount,
            Recurring = import.Recurring,
            Amount = import.Amount,
            Allocated = import.Allocated
        };

        _unitOfWork.ExpenseRepository.Add(expenseEntity);

        return expenseEntity;
    }

    private static void UpdateExistingExpense(ExpenseEntity entity, ExpenseCsvRow import)
    {
        entity.Description = import.Description;
        entity.NextDue = import.NextDue;
        entity.AccrualStart = import.AccrualStart;
        entity.Frequency = import.Frequency;
        entity.FrequencyCount = import.FrequencyCount;
        entity.Recurring = import.Recurring;
        entity.Amount = import.Amount;
        entity.Allocated = import.Allocated;    // TODO: ? leave these at their current values

        // Don't need to explicitly call _expenseRepository.Update(entity). The entity will
        // be marked as modified if anything has changed.
    }
}
