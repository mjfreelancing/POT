using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Microsoft.EntityFrameworkCore;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.ProblemDetails.Extensions;
using Pot.AspNetCore.Errors;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Create.Services;

internal sealed class CreateAccountService : ICreateAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public CreateAccountService(IAccountRepository accountRepository, ILogger<CreateAccountService> logger)
    {
        _accountRepository = accountRepository.WhenNotNull(); ;
        _logger = logger.WhenNotNull();
    }

    public async Task<EnrichedResult<AccountEntity>> CreateAccountAsync(Request request, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var account = new AccountEntity
        {
            Bsb = request.Bsb,
            Number = request.Number,
            Description = request.Description,
            Balance = request.Balance,
            Reserved = request.Reserved
        };




        var accountExists = await _accountRepository
            .AccountExistsAsync(account.Bsb, account.Number, cancellationToken)
            .ConfigureAwait(false);

        if (accountExists)
        {
            var problemDetails = ProblemDetailsFactory.CreateEntityExistsConflict(
                account,
                $"{nameof(AccountEntity.Bsb)}, {nameof(AccountEntity.Number)}",
                $"{account.Bsb}, {account.Number}");

            _logger.LogErrors(problemDetails);

            var accountError = new ServiceError(problemDetails);

            return EnrichedResult.Fail<AccountEntity>(accountError);
        }





        // TODO: Specification pattern
        var descriptionExists = await _accountRepository.Query()
            .AnyAsync(account => !(account.Bsb == request.Bsb && account.Number == request.Number) && account.Description == request.Description, cancellationToken)
            .ConfigureAwait(false);

        if (descriptionExists)
        {
            var problemDetails = ProblemDetailsFactory.CreateEntityExistsConflict(
                account,
                nameof(AccountEntity.Description),
                account.Description);

            _logger.LogErrors(problemDetails);

            var accountError = new ServiceError(problemDetails);

            return EnrichedResult.Fail<AccountEntity>(accountError);
        }






        await _accountRepository
            .AddAndSaveAsync(account, cancellationToken)
            .ConfigureAwait(false);

        return EnrichedResult.Success(account);
    }
}
