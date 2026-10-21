namespace Pot.RazorComponents.Models;

/// <summary>
/// The content of the message sent when a user asks to reset their password. One email address can belong to
/// accounts in several sites, so the message names the account and its site.
/// </summary>
public sealed class EmailChangePasswordInfo : EmailConfigBase
{
    /// <summary>Gets the display name of the recipient.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string DisplayName { get; init; }

    /// <summary>Gets the name of the site the account belongs to.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string SiteName { get; init; }

    /// <summary>Gets the code that matches this message to the verification screen.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string ReferenceCode { get; init; }

    /// <summary>Gets the code to enter on the verification screen.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string VerificationCode { get; init; }

    /// <summary>Gets the number of minutes the verification code remains valid.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required int OtpExpiryMinutes { get; init; }

    /// <summary>Gets the temporary password to sign in with after verification.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string TempPassword { get; init; }

    /// <summary>Gets the local date and time of the request.</summary>
    [EmailFormat(EmailFormatType.None)]
    public required DateTime RequestedAt { get; init; }

    /// <inheritdoc />
    [EmailFormat(EmailFormatType.None)]
    public override string RecipientName => DisplayName;

    /// <summary>Gets the account as "Display Name (username)".</summary>
    [EmailFormat(EmailFormatType.Both)]
    public string AccountLabel => FormatUserLabel(DisplayName, Username);

    /// <summary>Gets the formatted local date and time of the request.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public string RequestedAtLabel => FormatDateTime(RequestedAt);
}
