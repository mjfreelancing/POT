using AllOverIt.Patterns.Result;
using Pot.App.Features.Auth.PasswordReset.Verify.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Auth.PasswordReset.Verify;

public interface IVerifyPasswordResetService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> VerifyResetAsync(Input input, CancellationToken cancellationToken);
}
