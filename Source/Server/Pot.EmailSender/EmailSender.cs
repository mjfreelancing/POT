using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using Pot.EmailSender.Configuration;
using Pot.RazorComponents;
using Pot.RazorComponents.Models;

namespace Pot.EmailSender;

internal sealed class EmailSender : IEmailSender
{
    private const string ChangePasswordSubject = "POT request to change password";

    private readonly IRazorComponentRenderer _razorRenderer;
    private readonly SmtpConfiguration _smtpConfiguration;
    private readonly ILogger _logger;

    public EmailSender(IRazorComponentRenderer razorRenderer, SmtpConfiguration smtpConfiguration, ILogger<EmailSender> logger)
    {
        _razorRenderer = razorRenderer.WhenNotNull();
        _smtpConfiguration = smtpConfiguration.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async Task SendVerifyPasswordAsync(VerifyPasswordEmailConfig config, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { config.Username, config.ReferenceCode, config.OtpExpiryMinutes });

        var dictionary = config.ToPropertyDictionary();

        var html = await _razorRenderer.RenderToHtmlAsync<VerifyPasswordEmail>(dictionary);

        var message = new MimeMessage
        {
            Subject = ChangePasswordSubject
        };

        message.From.Add(new MailboxAddress(_smtpConfiguration.From.Name, _smtpConfiguration.From.Address));
        message.To.Add(new MailboxAddress(config.Username, config.Email));

        var bodyBuilder = new BodyBuilder
        {
            TextBody = html,
            HtmlBody = html
        };

        message.Body = bodyBuilder.ToMessageBody();

        var smtp = new SmtpClient
        {
            RequireTLS = true
        };

        await smtp.ConnectAsync(_smtpConfiguration.Host, _smtpConfiguration.Port, cancellationToken: cancellationToken);
        await smtp.AuthenticateAsync(_smtpConfiguration.Authentication.Username, _smtpConfiguration.Authentication.Password, cancellationToken);
        await smtp.SendAsync(message, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}
