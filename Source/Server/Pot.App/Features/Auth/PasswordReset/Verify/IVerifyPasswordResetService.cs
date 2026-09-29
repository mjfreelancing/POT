using AllOverIt.Patterns.Result;
using Pot.App.Features.Auth.PasswordReset.Verify.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Auth.PasswordReset.Verify;

/// <summary>
/// Verifies a password reset code and applies the temporary password carried by the matching request.
/// </summary>
public interface IVerifyPasswordResetService : IPotScopedDependency
{
    /// <summary>
    /// Verifies the reset code for the input's username and reference code.
    /// </summary>
    /// <param name="input">The username, reference code, and verification code to validate.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// An <c>EnrichedResult&lt;Output&gt;</c> reporting the verification outcome. A failed verification is returned
    /// as a successful result whose output status is invalid, expired, or too many attempts, rather than as an
    /// error result.
    /// </returns>
    Task<EnrichedResult<Output>> VerifyResetAsync(Input input, CancellationToken cancellationToken);
}
