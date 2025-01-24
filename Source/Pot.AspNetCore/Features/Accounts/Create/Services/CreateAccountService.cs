using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using AllOverIt.Patterns.Result;
using Pot.AspNetCore.Errors;
using Pot.AspNetCore.ProblemDetails.Extensions;
using Pot.AspNetCore.Validation;
using Pot.AspNetCore.Validation.Extensions;
using Pot.Data.Entities;
using Pot.Data.Repositories.Accounts;

namespace Pot.AspNetCore.Features.Accounts.Create.Services;

internal sealed class CreateAccountService : ICreateAccountService
{
    private readonly IProblemDetailsInspector _problemDetailsInspector;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<CreateAccountService> _logger;

    public CreateAccountService(IProblemDetailsInspector problemDetailsInspector, IAccountRepository accountRepository,
        ILogger<CreateAccountService> logger)
    {
        _problemDetailsInspector = problemDetailsInspector.WhenNotNull();
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

        var problemDetails = await _problemDetailsInspector
            .ValidateAsync(account, cancellationToken)
            .ConfigureAwait(false);

        if (problemDetails.IsProblem())
        {
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
