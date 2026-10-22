using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Pot.App.Errors;
using Pot.App.Features.Me.Delete.Models;

namespace Pot.App.Features.Me.Delete.EntityChecks.Checks;

/// <summary>
/// Rejects an account deletion when the typed username is not the caller's own.
/// </summary>
/// <remarks>
/// The typed value exists to make the caller stop and read what they are doing; it is compared trimmed and
/// case-insensitively, matching the case-insensitive usernames the database stores.
/// </remarks>
internal sealed class CheckUsernameMatches : AccountDeleteCheckBase
{
    private readonly ILogger _logger;

    public CheckUsernameMatches(ILogger<CheckUsernameMatches> logger)
    {
        _logger = logger.WhenNotNull();
    }

    /// <inheritdoc />
    public override int Order => 1;

    /// <summary>
    /// Fails when the typed username is not the caller's, otherwise delegates to the next check.
    /// </summary>
    /// <param name="state">The caller and the typed username.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The failing validation error, or <see langword="null" /> when the rule is satisfied.</returns>
    public override async Task<ApiDetailError?> HandleAsync(InputState state, CancellationToken cancellationToken)
    {
        _logger.LogCall(this);

        var matches = string.Equals(
            state.Username.Trim(),
            state.Caller.Username.Trim(),
            StringComparison.OrdinalIgnoreCase);

        if (!matches)
        {
            return ApiDetailErrorFactory.CreateBadRequestError(
                nameof(Input.Username),
                "The username does not match your account.");
        }

        return await base.HandleAsync(state, cancellationToken);
    }
}
