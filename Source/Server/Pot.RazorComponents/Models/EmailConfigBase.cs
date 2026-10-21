namespace Pot.RazorComponents.Models;

public abstract class EmailConfigBase
{
    [EmailFormat(EmailFormatType.Both)]
    public required string Username { get; init; }

    [EmailFormat(EmailFormatType.Both)]
    public required string Email { get; init; }

    /// <summary>Gets the name shown beside the recipient's address in the message's To header.</summary>
    [EmailFormat(EmailFormatType.None)]
    public virtual string RecipientName => Username;

    /// <summary>
    /// Formats a user as "Display Name (username)" so that two users with the same display name can be told apart.
    /// The username is shown on its own when the display name is the same as the username.
    /// </summary>
    /// <param name="displayName">The user's display name.</param>
    /// <param name="username">The user's unique username.</param>
    /// <returns>The display name followed by the username in parentheses, or the username alone.</returns>
    protected static string FormatUserLabel(string displayName, string username)
    {
        return string.Equals(displayName, username, StringComparison.OrdinalIgnoreCase)
            ? username
            : $"{displayName} ({username})";
    }
}
