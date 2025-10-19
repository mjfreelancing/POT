using Pot.RazorComponents.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.EmailSender;

public interface IEmailSender : IPotScopedDependency
{
    Task SendVerifyPasswordAsync(VerifyPasswordEmailConfig config, CancellationToken cancellationToken);
}
