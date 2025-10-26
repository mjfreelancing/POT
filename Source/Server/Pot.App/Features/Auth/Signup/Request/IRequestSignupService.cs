using AllOverIt.Patterns.Result;
using Pot.App.Features.Auth.Signup.Request.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Auth.Signup.Request;

public interface IRequestSignupService : IPotScopedDependency
{
    Task<EnrichedResult<Output>> RequestSignupAsync(Input input, CancellationToken cancellationToken);
}
