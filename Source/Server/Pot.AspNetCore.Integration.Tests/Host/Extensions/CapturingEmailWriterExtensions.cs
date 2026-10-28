using AllOverIt.Assertion;
using Pot.RazorComponents.Models;

namespace Pot.AspNetCore.Integration.Tests.Host.Extensions;

/// <summary>
/// Reads the secret out of a captured message, so no test has to know how a message is laid out.
/// </summary>
public static class CapturingEmailWriterExtensions
{
    /// <summary>
    /// Reads the temporary password from the invitation sent to an address.
    /// </summary>
    /// <param name="emails">The captured messages.</param>
    /// <param name="recipient">The address the invitation was sent to.</param>
    /// <returns>The temporary password the invitation carries.</returns>
    /// <remarks>
    /// A missing message is reported with what was captured instead, so a provisioning problem reads as itself
    /// rather than as a null reference inside the test that follows.
    /// </remarks>
    public static string GetInvitationPassword(this CapturingEmailWriter emails, string recipient)
    {
        _ = emails.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);

        var invitation = emails.Latest(EmailType.Invitation, recipient);

        if (invitation is null)
        {
            var captured = string.Join(", ", emails.Snapshot().Select(message => $"{message.Type} to {message.Content.Email}"));

            throw new InvalidOperationException(
                $"No invitation was captured for '{recipient}'. Captured: {(captured.Length == 0 ? "nothing" : captured)}.");
        }

        return ((EmailInvitationInfo)invitation.Content).TempPassword;
    }

    /// <summary>
    /// Reads the temporary password from the message a sign-up sent to an address.
    /// </summary>
    /// <param name="emails">The captured messages.</param>
    /// <param name="recipient">The address the sign-up message was sent to.</param>
    /// <returns>The temporary password the message carries.</returns>
    /// <remarks>
    /// A sign-up generates a password, stores only its hash, and sends the password itself. Without this helper a
    /// test could complete a sign-up but never sign in as the account it created.
    /// </remarks>
    public static string GetSignUpPassword(this CapturingEmailWriter emails, string recipient)
    {
        _ = emails.WhenNotNull();
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);

        var signUp = emails.Latest(EmailType.Signup, recipient);

        if (signUp is null)
        {
            var captured = string.Join(", ", emails.Snapshot().Select(message => $"{message.Type} to {message.Content.Email}"));

            throw new InvalidOperationException(
                $"No sign-up message was captured for '{recipient}'. Captured: {(captured.Length == 0 ? "nothing" : captured)}.");
        }

        return ((EmailOtpInfo)signUp.Content).TempPassword;
    }
}
