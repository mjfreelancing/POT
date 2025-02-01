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
    private sealed record ExpenseKey(string? AccountId, string Description);

    private readonly IExpenseImportValidator _expenseImportValidator;
    private readonly IPotUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ImportExpenseService(IExpenseImportValidator expenseImportValidator, IPotUnitOfWork unitOfWork,
        ILogger<ImportExpenseService> logger)
    {
        _expenseImportValidator = expenseImportValidator.WhenNotNull();
        _unitOfWork = unitOfWork.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<ImportSummary>> ImportExpensesAsync(IEnumerable<ExpenseForImport> expensesForImport, bool overwrite,
        CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { overwrite });

        using (_unitOfWork.WithTracking())
        {
            var problemDetailsErrors = new List<CsvProblemDetailsError>();

            // Look for duplicates in the import file - not checking for duplicates in the database since
            // the import will either skip or overwrite existing records.
            var expenseKeys = new HashSet<ExpenseKey>();

            var recordCount = 0;
            var imported = 0;
            var updated = 0;
            var row = 1;            // Skip the header row

            foreach (var import in expensesForImport)
            {
                row++;
                recordCount++;

                CheckImportColumns(row, import, problemDetailsErrors);
                CheckForDuplicateExpense(row, import, expenseKeys, problemDetailsErrors);

                var (accountGuid, accountEntity) = await CheckExpenseAccountAsync(row, import, problemDetailsErrors, cancellationToken)
                    .ConfigureAwait(false);

                // If an invalid row is detected, only look for more errors.
                if (problemDetailsErrors.Count > 0)
                {
                    continue;
                }

                var existingExpense = await GetExpenseOrDefaultAsync(import, accountGuid, accountEntity, cancellationToken)
                    .ConfigureAwait(false);

                if (existingExpense is null)
                {
                    AddNewExpense(import, accountEntity);
                    imported++;
                }
                else if (overwrite)
                {
                    UpdateExistingExpense(existingExpense, import);
                    updated++;
                }
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
                Skipped = recordCount - imported - updated,
                Imported = imported,
                Updated = updated,
                Total = recordCount
            };

            return EnrichedResult.Success(result);
        }
    }

    private async Task<ExpenseEntity?> GetExpenseOrDefaultAsync(ExpenseForImport import, Guid accountGuid, AccountEntity? accountEntity,
        CancellationToken cancellationToken)
    {
        ExpenseEntity? existing = null;

        if (accountEntity is null)
        {
            existing = await _unitOfWork.ExpenseRepository
                .Where(expense => expense.Account == null && expense.Description == import.Description)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            existing = await _unitOfWork.ExpenseRepository
                .Where(expense => expense.Account != null && expense.Account.RowId == accountGuid && expense.Description == import.Description)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return existing;
    }

    private void CheckImportColumns(int row, ExpenseForImport import, List<CsvProblemDetailsError> problemDetailsErrors)
    {
        var validationResult = _expenseImportValidator.Validate(import);

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

    private static void CheckForDuplicateExpense(int row, ExpenseForImport import, HashSet<ExpenseKey> expenseKeys,
        List<CsvProblemDetailsError> problemDetailsErrors)
    {
        var expenseKey = new ExpenseKey(import.AccountId, import.Description);

        // Look for a duplicate import row
        if (!expenseKeys.Add(expenseKey))
        {
            var errorDetails = new CsvProblemDetailsError
            {
                ImportRow = row,
                PropertyName = $"{nameof(ExpenseForImport.AccountId)}, {nameof(ExpenseForImport.Description)}",
                ErrorCode = ErrorCodes.Duplicate,
                AttemptedValue = $"{import.AccountId}, {import.Description}",
                ErrorMessage = "The import data contains a duplicate row with the same Account and Description"
            };

            problemDetailsErrors.Add(errorDetails);
        }
    }

    private async Task<(Guid, AccountEntity?)> CheckExpenseAccountAsync(int row, ExpenseForImport import,
        List<CsvProblemDetailsError> problemDetailsErrors, CancellationToken cancellationToken)
    {
        Guid accountGuid = default;
        AccountEntity? accountEntity = null;

        if (import.AccountId is not null)
        {
            accountGuid = import.AccountId.As<Guid>();

            accountEntity = await _unitOfWork.AccountRepository
                .GetAccountOrDefaultAsync(accountGuid, cancellationToken)
                .ConfigureAwait(false);

            if (accountEntity is null)
            {
                var errorDetails = new CsvProblemDetailsError
                {
                    ImportRow = row,
                    PropertyName = nameof(ExpenseForImport.AccountId),
                    ErrorCode = ErrorCodes.NotFound,
                    AttemptedValue = import.AccountId,
                    ErrorMessage = "The Account does not exist"
                };

                problemDetailsErrors.Add(errorDetails);
            }
        }

        return (accountGuid, accountEntity);
    }

    private static ExpenseEntity CreateExpenseEntity(ExpenseForImport import)
    {
        return new ExpenseEntity
        {
            Description = import.Description,
            NextDue = import.NextDue,
            AccrualStart = import.AccrualStart,
            Frequency = import.Frequency,
            FrequencyCount = import.FrequencyCount,
            Recurring = import.Recurring,
            Amount = import.Amount,
            Allocated = import.Allocated
        };
    }

    private void AddNewExpense(ExpenseForImport import, AccountEntity? accountEntity)
    {
        var expenseEntity = CreateExpenseEntity(import);

        if (accountEntity is not null)
        {
            accountEntity.Expenses.Add(expenseEntity);
        }
        else
        {
            _unitOfWork.ExpenseRepository.Add(expenseEntity);
        }
    }

    private static void UpdateExistingExpense(ExpenseEntity entity, ExpenseForImport import)
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
