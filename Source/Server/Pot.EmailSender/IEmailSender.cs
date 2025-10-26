using Pot.RazorComponents.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.EmailSender;

public interface IEmailSender : IPotScopedDependency
{
    Task SendChangePasswordEmailAsync(EmailOtpInfo config, CancellationToken cancellationToken);
    Task SendSignupEmailAsync(EmailOtpInfo config, CancellationToken cancellationToken);
}
