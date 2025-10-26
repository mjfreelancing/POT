using AllOverIt.Patterns.Result;
using Pot.App.Features.Auth.Signup.Complete.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Auth.Signup.Complete;

public interface IVerifySignupService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> VerifySignupAsync(Input input, CancellationToken cancellationToken);
}
