using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Concerns.Validation.Extensions;
using Pot.AspNetCore.Errors;
using Pot.AspNetCore.Features.Accounts.Import.Models;
using Pot.AspNetCore.Features.Accounts.Import.Validators;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Import.Services;

internal sealed class ImportAccountService : IImportAccountService
{
    private sealed record AccountKey(string Bsb, string Number);

    private readonly IAccountForImportValidator _accountImportValidator;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public ImportAccountService(IAccountForImportValidator accountImportValidator, IAccountRepository accountRepository, ILogger<ImportAccountService> logger)
    {
        _accountImportValidator = accountImportValidator.WhenNotNull();
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<ImportSummary>> ImportAccountsAsync(IEnumerable<AccountForImport> accountsForImport, bool overwrite, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { overwrite });

        using (_accountRepository.WithTracking())
        {
            var problemDetailsErrors = new List<CsvProblemDetailsError>();

            // Look for duplicates in the import file - not checking for duplicates in the database since
            // the import will either skip or overwrite existing records.
            var accountKeys = new HashSet<AccountKey>();

            var recordCount = 0;
            var imported = 0;
            var updated = 0;
            var row = 1;            // Skip the header row

            foreach (var import in accountsForImport)
            {
                row++;
                recordCount++;

                CheckImportColumns(row, import, problemDetailsErrors);
                CheckForDuplicateAccount(row, import, accountKeys, problemDetailsErrors);

                // If an invalid row is detected, only look for more errors.
                if (problemDetailsErrors.Count > 0)
                {
                    continue;
                }

                var existingAccount = await _accountRepository
                    .GetAccountOrDefaultAsync(import.Bsb, import.Number, cancellationToken)
                    .ConfigureAwait(false);

                if (existingAccount is null)
                {
                    AddAccountEntity(import);
                    imported++;
                }
                else if (overwrite)
                {
                    UpdateAccountEntity(existingAccount, import);
                    updated++;
                }
            }

            if (problemDetailsErrors.Count > 0)
            {
                var problemDetails = ApiProblemDetailsFactory.CreateUnprocessableEntity(problemDetailsErrors);
                var serviceError = new ServiceError(problemDetails);

                return EnrichedResult.Fail<ImportSummary>(serviceError);
            }

            await _accountRepository
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

    private void CheckImportColumns(int row, AccountForImport import, List<CsvProblemDetailsError> problemDetailsErrors)
    {
        var validationResult = _accountImportValidator.Validate(import);

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

    private static void CheckForDuplicateAccount(int row, AccountForImport import, HashSet<AccountKey> accountKeys,
        List<CsvProblemDetailsError> problemDetailsErrors)
    {
        var accountKey = new AccountKey(import.Bsb, import.Number);

        // Look for a duplicate import row
        if (!accountKeys.Add(accountKey))
        {
            var errorDetails = new CsvProblemDetailsError
            {
                ImportRow = row,
                PropertyName = $"{nameof(AccountForImport.Bsb)}, {nameof(AccountForImport.Number)}",
                ErrorCode = ErrorCodes.Duplicate,
                AttemptedValue = $"{import.Bsb}, {import.Number}",
                ErrorMessage = "The import data contains a duplicate row with the same BSB and Number"
            };

            problemDetailsErrors.Add(errorDetails);
        }
    }

    private void AddAccountEntity(AccountForImport import)
    {
        var newAccount = new AccountEntity
        {
            Bsb = import.Bsb,
            Number = import.Number,
            Description = import.Description,
            Balance = import.Balance,
            Reserved = import.Reserved,
            Allocated = 0.0d,
            DailyAccrual = 0.0d
        };

        _accountRepository.Add(newAccount);
    }

    private static void UpdateAccountEntity(AccountEntity entity, AccountForImport import)
    {
        entity.Bsb = import.Bsb;
        entity.Number = import.Number;
        entity.Description = import.Description;
        entity.Balance = import.Balance;
        entity.Reserved = import.Reserved;

        // Leave these at their current values
        // entity.Allocated
        // entity.DailyAccrual

        // Don't need to explicitly call _accountRepository.Update(entity). The entity will
        // be marked as modified if anything has changed.
    }
}
