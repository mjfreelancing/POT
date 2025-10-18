using Pot.RazorComponents.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Concerns.Email;

public interface IEmailSender : IPotScopedDependency
{
    Task SendVerifyPasswordAsync(VerifyPasswordEmailConfig config, CancellationToken cancellationToken);
}
