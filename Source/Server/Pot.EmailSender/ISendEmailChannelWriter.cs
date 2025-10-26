using Pot.RazorComponents.Models;

namespace Pot.EmailSender;

public interface ISendEmailChannelWriter
{
    ValueTask SubmitAsync(EmailType emailType, EmailOtpInfo emailConfig, CancellationToken cancellationToken);
}
