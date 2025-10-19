using Pot.RazorComponents.Models;

namespace Pot.EmailSender;

public interface ISendEmailChannelWriter
{
    ValueTask SubmitAsync(VerifyPasswordEmailConfig emailConfig, CancellationToken cancellationToken);
}
