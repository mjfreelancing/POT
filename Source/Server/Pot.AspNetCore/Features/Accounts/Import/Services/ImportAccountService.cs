using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.Validation.Extensions;
using Pot.AspNetCore.Errors;
using Pot.AspNetCore.Features.Accounts.Import.Models;
using Pot.AspNetCore.Features.Accounts.Import.Validators;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Import.Services;

internal sealed class ImportAccountService : IImportAccountService
{
    private record AccountEntityInfo(AccountEntity AccountEntity, bool Created);

    private readonly IAccountCsvRowValidator _csvRowValidator;
    private readonly IPersistableAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public ImportAccountService(IAccountCsvRowValidator csvRowValidator, IPersistableAccountRepository accountRepository, ILogger<ImportAccountService> logger)
    {
        _csvRowValidator = csvRowValidator.WhenNotNull();
        _accountRepository = accountRepository.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<ImportSummary>> ImportAccountsAsync(IEnumerable<AccountCsvRow> csvRows, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        using (_accountRepository.WithTracking())
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

                var accountEntityInfo = await CreateOrUpdateAccountAsync(csvRow, cancellationToken);

                if (accountEntityInfo.Created)
                {
                    imported++;
                }
                else
                {
                    updated++;
                }
            }

            if (problemDetailsErrors.Count > 0)
            {
                var problemDetails = ApiProblemDetailsFactory.CreateUnprocessableEntity(problemDetailsErrors);
                var serviceError = new ServiceError(problemDetails);

                return EnrichedResult.Fail<ImportSummary>(serviceError);
            }

            // Could throw UniqueConstraintException (or a related database exception),
            // resulting in a custom 422 ProblemDetails response.
            await _accountRepository
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

    private void CheckImportColumns(int row, AccountCsvRow import, List<CsvProblemDetailsError> problemDetailsErrors)
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

    private async Task<AccountEntityInfo> CreateOrUpdateAccountAsync(AccountCsvRow csvRow, CancellationToken cancellationToken)
    {
        AccountEntity? accountEntity = null;

        var csvAccountId = csvRow.Id;

        if (csvAccountId.HasValue)
        {
            // Will not exist if re-importing a file to recover data
            accountEntity = await _accountRepository
                .GetAccountOrDefaultAsync(csvAccountId.Value, cancellationToken)
                .ConfigureAwait(false);
        }

        if (accountEntity is null)
        {
            accountEntity = CreateAccountEntity(csvAccountId, csvRow);

            return new AccountEntityInfo(accountEntity, true);
        }

        UpdateExistingAccount(accountEntity, csvRow);

        return new AccountEntityInfo(accountEntity, false);
    }

    private AccountEntity CreateAccountEntity(Guid? accountId, AccountCsvRow import)
    {
        var accountEntity = new AccountEntity
        {
            RowId = accountId ?? Guid.NewGuid(),
            Bsb = import.Bsb,
            Number = import.Number,
            Description = import.Description,
            Balance = import.Balance,
            Reserved = import.Reserved,
            Allocated = 0.0d,
            DailyAccrual = 0.0d
        };

        _accountRepository.Add(accountEntity);

        return accountEntity;
    }

    private static void UpdateExistingAccount(AccountEntity entity, AccountCsvRow import)
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
