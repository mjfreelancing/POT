using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MimeKit;
using Pot.EmailSender.Configuration;
using Pot.RazorComponents;
using Pot.RazorComponents.Emails.ApprovalAccepted;
using Pot.RazorComponents.Emails.ApprovalRejected;
using Pot.RazorComponents.Emails.BudgetReminder;
using Pot.RazorComponents.Emails.ChangePassword;
using Pot.RazorComponents.Emails.Invitation;
using Pot.RazorComponents.Emails.PendingApproval;
using Pot.RazorComponents.Emails.Signup;
using Pot.RazorComponents.Models;
using System.Reflection;

namespace Pot.EmailSender;

internal sealed class EmailSender : IEmailSender
{
    private const string ChangePasswordSubject = "POT - Change Password";
    private const string SignupSubject = "POT - Signup";
    private const string InvitationSubject = "POT - Invitation";
    private const string PendingApprovalSubject = "POT - Pending Approval";
    private const string ApprovalAcceptedSubject = "POT - Approval Accepted";
    private const string ApprovalRejectedSubject = "POT - Approval Denied";
    private const string BudgetReminderSubject = "POT - Budget Reminder";

    private readonly IRazorComponentRenderer _razorRenderer;
    private readonly SmtpConfiguration _smtpConfiguration;
    private readonly ILogger _logger;

    public EmailSender(IRazorComponentRenderer razorRenderer, SmtpConfiguration smtpConfiguration, ILogger<EmailSender> logger)
    {
        _razorRenderer = razorRenderer.WhenNotNull();
        _smtpConfiguration = smtpConfiguration.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public Task SendChangePasswordEmailAsync(EmailOtpInfo config, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { config.Username, config.ReferenceCode, config.OtpExpiryMinutes });

        return SendEmailAsync<ChangePasswordEmail>(config, ChangePasswordSubject, PlainTextEmailTemplateLoader.ChangePassword, cancellationToken);
    }

    public Task SendSignupEmailAsync(EmailOtpInfo config, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { config.Username, config.ReferenceCode, config.OtpExpiryMinutes });

        return SendEmailAsync<SignupEmail>(config, SignupSubject, PlainTextEmailTemplateLoader.Signup, cancellationToken);
    }

    public Task SendInvitationEmailAsync(EmailInvitationInfo config, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { config.Username });

        return SendEmailAsync<InvitationEmail>(config, InvitationSubject, PlainTextEmailTemplateLoader.Invitation, cancellationToken);
    }

    public Task SendPendingApprovalEmailAsync(EmailPendingApprovalInfo config, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { config.Username, config.UserUsername });

        return SendEmailAsync<PendingApprovalEmail>(config, PendingApprovalSubject, PlainTextEmailTemplateLoader.PendingApproval, cancellationToken);
    }

    public Task SendApprovalAcceptedEmailAsync(EmailApprovalStatusInfo config, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { config.Username });

        return SendEmailAsync<ApprovalAcceptedEmail>(config, ApprovalAcceptedSubject, PlainTextEmailTemplateLoader.ApprovalAccepted, cancellationToken);
    }

    public Task SendApprovalRejectedEmailAsync(EmailApprovalStatusInfo config, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { config.Username });

        return SendEmailAsync<ApprovalRejectedEmail>(config, ApprovalRejectedSubject, PlainTextEmailTemplateLoader.ApprovalRejected, cancellationToken);
    }

    public Task SendBudgetReminderEmailAsync(EmailBudgetReminderInfo config, CancellationToken cancellationToken)
    {
        _logger.LogCall(this, new { config.Username, IncomeCount = config.UserIncomes.Count, ExpenseCount = config.UserExpenses.Count });

        return SendEmailAsync<BudgetReminderEmail>(config, BudgetReminderSubject, PlainTextEmailTemplateLoader.BudgetReminder, cancellationToken);
    }

    private async Task SendEmailAsync<TEmailComponent>(EmailConfigBase config, string subject, string plainTextTemplateName,
        CancellationToken cancellationToken) where TEmailComponent : IComponent
    {
        var dictionary = config.ToPropertyDictionary();
        dictionary.Remove(nameof(EmailConfigBase.Email));   // Not used in the emails

        var message = new MimeMessage
        {
            Subject = subject
        };

        message.From.Add(new MailboxAddress(_smtpConfiguration.From.Name, _smtpConfiguration.From.Address));
        message.To.Add(new MailboxAddress(config.Username, config.Email));

        var htmlDictionary = FilterPropertiesByFormat(dictionary, config.GetType(), EmailFormatType.Html);
        var html = await _razorRenderer.RenderToHtmlAsync<TEmailComponent>(htmlDictionary);

        var plainTextDictionary = FilterPropertiesByFormat(dictionary, config.GetType(), EmailFormatType.PlainText);
        var plainText = PlainTextEmailTemplateLoader.Populate(plainTextTemplateName, plainTextDictionary);

        var bodyBuilder = new BodyBuilder
        {
            TextBody = plainText,
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

    private static Dictionary<string, object?> FilterPropertiesByFormat(IDictionary<string, object?> dictionary, Type configType, EmailFormatType targetFormat)
    {
        var filteredDictionary = new Dictionary<string, object?>();
        var properties = configType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var kvp in dictionary)
        {
            var property = properties
                .SingleOrDefault(p => p.Name == kvp.Key)
                .WhenNotNull(errorMessage: $"Property '{kvp.Key}' not found on type '{configType.Name}'");

            var attribute = property
                .GetCustomAttribute<EmailFormatAttribute>()
                .WhenNotNull(errorMessage: $"Property '{kvp.Key}' on type '{configType.Name}' is missing the [EmailFormat] attribute");

            if (attribute.Format.HasFlag(targetFormat))
            {
                filteredDictionary[kvp.Key] = kvp.Value;
            }
        }

        return filteredDictionary;
    }
}
