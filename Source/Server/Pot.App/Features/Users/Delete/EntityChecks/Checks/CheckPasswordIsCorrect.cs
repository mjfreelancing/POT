using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Concerns.Auth;
using Pot.App.Errors;
using Pot.App.Features.Users.Delete.Models;

namespace Pot.App.Features.Users.Delete.EntityChecks.Checks;

/// <summary>
/// Rejects a deletion when the caller's confirmation password is wrong.
/// </summary>
/// <remarks>
/// Runs first so that a caller who does not know the password learns nothing about the target's rules. The
/// password itself is never echoed into the error.
/// </remarks>
internal sealed class CheckPasswordIsCorrect : UserDeleteCheckBase
{
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly ILogger _logger;

    public CheckPasswordIsCorrect(IUserPasswordHasher passwordHasher, ILogger<CheckPasswordIsCorrect> logger)
    {
        _passwordHasher = passwordHasher.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public override int Order => 0;

    /// <summary>
    /// Fails when the password does not match the caller's stored hash, otherwise delegates to the next check.
    /// </summary>
    /// <param name="state">The caller, the target and the confirmation password.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null" /> when the rule is satisfied.</returns>
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var isValid = _passwordHasher.IsValidPasswordHash(state.Caller, state.Password, state.Caller.PasswordHash);

        if (!isValid)
        {
            return ApiDetailErrorFactory.CreateBadRequestError(
                nameof(Input.Password),
                attemptedValue: string.Empty,
                "The password is incorrect.");
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
