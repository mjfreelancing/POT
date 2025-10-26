using AllOverIt.Assertion;
using AllOverIt.Logging.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pot.RazorComponents.Models;
using System.Threading.Channels;

namespace Pot.EmailSender;

internal sealed class SendEmailChannel : ISendEmailChannelReader, ISendEmailChannelWriter
{
    private sealed class EmailChannelConfig
    {
        public EmailType EmailType { get; init; }
        public required EmailOtpInfo EmailInfo { get; init; }
    }

    private const int DelaySeconds = 30;

    private readonly Channel<EmailChannelConfig> _channel;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SendEmailChannel(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory.WhenNotNull();

        _channel = Channel.CreateUnbounded<EmailChannelConfig>(new UnboundedChannelOptions
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

                using var scope = _serviceScopeFactory.CreateScope();

                logger = GetLogger(scope);

                if (!_channel.Reader.TryRead(out var channelConfig))
                {
                    logger.LogError("The email channel reader failed to read from the channel.");

                    // Prevent tight loop
                    await Task.Delay(DelaySeconds * 1000, cancellationToken);

                    continue;
                }

                logger.LogCall(this, new { channelConfig.EmailType, channelConfig.EmailInfo.Username });

                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                await SendEmailAsync(emailSender, channelConfig, cancellationToken).ConfigureAwait(false);
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

    public ValueTask SubmitAsync(EmailType emailType, EmailOtpInfo emailConfig, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var logger = GetLogger(scope);
        logger.LogCall(this, new { emailType, emailConfig.Username });

        var channelConfig = new EmailChannelConfig
        {
            EmailType = emailType,
            EmailInfo = emailConfig
        };

        return _channel.Writer.WriteAsync(channelConfig, cancellationToken);
    }

    private static ILogger GetLogger(IServiceScope scope)
    {
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

        return loggerFactory.CreateLogger<SendEmailChannel>();
    }

    private static Task SendEmailAsync(IEmailSender emailSender, EmailChannelConfig channelConfig, CancellationToken cancellationToken)
    {
        Func<EmailOtpInfo, CancellationToken, Task> emailAction = channelConfig.EmailType switch
        {
            EmailType.ChangePassword => emailSender.SendChangePasswordEmailAsync,
            EmailType.Signup => emailSender.SendSignupEmailAsync,
            _ => throw new NotSupportedException($"The email type '{channelConfig.EmailType}' is not supported.")
        };

        return emailAction.Invoke(channelConfig.EmailInfo, cancellationToken);
    }
}
