using Pot.RazorComponents.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.EmailSender;

public interface IEmailSender : IPotScopedDependency
{
    Task SendChangePasswordEmailAsync(EmailOtpInfo config, CancellationToken cancellationToken);
    Task SendSignupEmailAsync(EmailOtpInfo config, CancellationToken cancellationToken);
    Task SendInvitationEmailAsync(EmailInvitationInfo config, CancellationToken cancellationToken);
    Task SendPendingApprovalEmailAsync(EmailPendingApprovalInfo config, CancellationToken cancellationToken);
    Task SendApprovalAcceptedEmailAsync(EmailApprovalStatusInfo config, CancellationToken cancellationToken);
    Task SendApprovalRejectedEmailAsync(EmailApprovalStatusInfo config, CancellationToken cancellationToken);
}
