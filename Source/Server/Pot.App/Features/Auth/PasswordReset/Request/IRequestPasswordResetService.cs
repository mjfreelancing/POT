using Pot.App.Features.Auth.PasswordReset.Request.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Auth.PasswordReset.Request;

public interface IRequestPasswordResetService : IPotScopedDependency
{
    Task<string> RequestResetAsync(Input input, CancellationToken cancellationToken);
}
