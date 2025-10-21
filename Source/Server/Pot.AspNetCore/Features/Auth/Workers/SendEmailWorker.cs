using AllOverIt.Assertion;
using AllOverIt.GenericHost;
using Pot.EmailSender;

namespace Pot.AspNetCore.Features.Auth.Workers;

internal sealed class SendEmailWorker : BackgroundWorker
{
    private readonly ISendEmailChannelReader _sendEmailChannelReader;

    public SendEmailWorker(IHostApplicationLifetime applicationLifetime, ISendEmailChannelReader sendEmailChannelReader)
        : base(applicationLifetime)
    {
        _sendEmailChannelReader = sendEmailChannelReader.WhenNotNull();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _sendEmailChannelReader.ProcessEmailsAsync(stoppingToken);
    }
}
