using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pot.RazorComponents.Models;
using System.Threading.Channels;

namespace Pot.EmailSender;

internal sealed class SendEmailChannel : ISendEmailChannelReader, ISendEmailChannelWriter
{
    private const int DelaySeconds = 10;

    private readonly Channel<VerifyPasswordEmailConfig> _channel;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SendEmailChannel(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory.WhenNotNull();

        _channel = Channel.CreateUnbounded<VerifyPasswordEmailConfig>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true
        });
    }

    public async Task ProcessEmailsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ILogger? logger = null;

            try
            {
                await _channel.Reader.WaitToReadAsync(cancellationToken);

                if (!_channel.Reader.TryRead(out var emailConfig))
                {
                    // Prevent tight loop
                    await Task.Delay(DelaySeconds * 1000, cancellationToken);

                    // TODO: Log error
                    continue;
                }

                using var scope = _serviceScopeFactory.CreateScope();

                var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                logger = loggerFactory.CreateLogger<SendEmailChannel>();

                logger.LogCall(this, new { emailConfig.Username });

                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                await emailSender
                    .SendVerifyChangePasswordAsync(emailConfig, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                logger?.LogError(exception, "An error occurred during the Expired OTP cleanup process: {ExceptionMessage}", exception.Message);
            }
        }
    }

    public ValueTask SubmitAsync(VerifyPasswordEmailConfig emailConfig, CancellationToken cancellationToken)
    {
        // TODO: ?? Add logging

        return _channel.Writer.WriteAsync(emailConfig, cancellationToken);
    }
}
