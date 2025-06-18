using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Validation;
using Pot.App.Errors;
using Pot.App.Features.Accounts.Import.Models;
using Pot.App.Features.Accounts.Import.Validators;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.App.Features.Accounts.Import;

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
            var problemDetailsErrors = new List<ValidationFailure>();

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
                var validationResult = new ValidationResult(problemDetailsErrors);
                var problemDetails = validationResult.ToProblemDetailsErrors();
                var problemCollection = new ProblemDetailsErrorCollection(ProblemType.UnprocessableEntity, problemDetails);

                return EnrichedResult.Fail<ImportSummary>(problemCollection);
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

    private void CheckImportColumns(int row, AccountCsvRow import, List<ValidationFailure> validationFailures)
    {
        var validationResult = _csvRowValidator.Validate(import);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                error.AddCustomState("row", row);
            }

            validationFailures.AddRange(validationResult.Errors);
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
            TotalExpenseAccrued = 0.0d,
            DailyExpenseAccrual = 0.0d
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
        // entity.TotalExpenseAccrued
        // entity.DailyExpenseAccrual

        // Don't need to explicitly call _accountRepository.Update(entity). The entity will
        // be marked as modified if anything has changed.
    }
}
