namespace Pot.EmailSender;

public interface ISendEmailChannelReader
{
    Task ProcessEmailsAsync(CancellationToken cancellationToken);
}
