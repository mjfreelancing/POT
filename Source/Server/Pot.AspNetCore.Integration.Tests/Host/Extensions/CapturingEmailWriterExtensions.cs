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
}
