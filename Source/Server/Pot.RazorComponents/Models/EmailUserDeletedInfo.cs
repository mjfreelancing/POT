using System.Globalization;
using System.Text;

namespace Pot.RazorComponents.Models;

/// <summary>
/// The content of the message sent when a user is deleted. It goes to the deleted user and to the site's
/// enabled administrators. The deleted user's copy addresses them directly, while the administrators' copies
/// name the deleted user.
/// </summary>
public sealed class EmailUserDeletedInfo : EmailConfigBase
{
    /// <summary>A labelled value shown in the message's deletion details.</summary>
    public sealed class DetailInfo
    {
        /// <summary>Gets the label of the detail.</summary>
        public required string Label { get; init; }

        /// <summary>Gets the value of the detail.</summary>
        public required string Value { get; init; }
    }

    private const string SelfDeletionLabel = "Self-deleted";

    /// <summary>Gets the display name of the recipient.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public required string DisplayName { get; init; }

    /// <summary>Gets the display name of the deleted user.</summary>
    [EmailFormat(EmailFormatType.None)]
    public required string DeletedUserDisplayName { get; init; }

    /// <summary>Gets the username of the deleted user.</summary>
    [EmailFormat(EmailFormatType.None)]
    public required string DeletedUsername { get; init; }

    /// <summary>Gets the display name of the user who performed the deletion.</summary>
    [EmailFormat(EmailFormatType.None)]
    public required string DeletedByDisplayName { get; init; }

    /// <summary>Gets the username of the user who performed the deletion. This is the deleted user's own username for a self-deletion.</summary>
    [EmailFormat(EmailFormatType.None)]
    public required string DeletedByUsername { get; init; }

    /// <summary>Gets the name of the site the user belonged to.</summary>
    [EmailFormat(EmailFormatType.None)]
    public required string SiteName { get; init; }

    /// <summary>Gets the local date and time of the deletion.</summary>
    [EmailFormat(EmailFormatType.None)]
    public required DateTime DeletedAt { get; init; }

    /// <inheritdoc />
    [EmailFormat(EmailFormatType.None)]
    public override string RecipientName => DisplayName;

    /// <summary>Gets whether this copy of the message is addressed to the deleted user.</summary>
    [EmailFormat(EmailFormatType.None)]
    public bool IsRecipientDeletedUser => Username == DeletedUsername;

    /// <summary>Gets whether the deleted user deleted their own account.</summary>
    [EmailFormat(EmailFormatType.None)]
    public bool IsSelfDeletion => DeletedByUsername == DeletedUsername;

    /// <summary>Gets the deleted user as "Display Name (username)".</summary>
    [EmailFormat(EmailFormatType.None)]
    public string DeletedUserLabel => FormatUserLabel(DeletedUserDisplayName, DeletedUsername);

    /// <summary>Gets who performed the deletion as "Display Name (username)", or a self-deletion label.</summary>
    [EmailFormat(EmailFormatType.None)]
    public string DeletedByLabel => IsSelfDeletion
        ? SelfDeletionLabel
        : FormatUserLabel(DeletedByDisplayName, DeletedByUsername);

    /// <summary>Gets the formatted local date and time of the deletion.</summary>
    [EmailFormat(EmailFormatType.None)]
    public string DeletedAtLabel => DeletedAt.ToString("ddd, dd MMM yyyy, h:mm tt", CultureInfo.InvariantCulture);

    /// <summary>Gets the message heading.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public string Heading => IsRecipientDeletedUser ? "Your Account Was Deleted" : "User Deleted";

    /// <summary>Gets the plain-text underline for the message title, sized to match it.</summary>
    [EmailFormat(EmailFormatType.PlainText)]
    public string HeadingUnderline => new('=', $"POT - {Heading}".Length);

    /// <summary>Gets the opening sentence of the message.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public string Introduction => GetIntroduction();

    /// <summary>Gets the deletion details shown to this recipient.</summary>
    [EmailFormat(EmailFormatType.Html)]
    public IReadOnlyList<DetailInfo> Details => GetDetails();

    /// <summary>Gets the deletion details shown to this recipient, as plain text.</summary>
    [EmailFormat(EmailFormatType.PlainText)]
    public string DetailsSection => BuildDetailsSection(Details);

    /// <summary>Gets the closing note of the message.</summary>
    [EmailFormat(EmailFormatType.Both)]
    public string ClosingNote => IsRecipientDeletedUser && IsSelfDeletion
        ? "This is an automated confirmation. If you did not request this, contact your site administrator."
        : "This is an automated confirmation. No further action is required.";

    private string GetIntroduction()
    {
        if (!IsRecipientDeletedUser)
        {
            return "A user account has been deleted.";
        }

        return IsSelfDeletion
            ? $"Your account on the site {SiteName} has been deleted, as you requested. You can no longer sign in."
            : $"Your account on the site {SiteName} has been deleted by an administrator. You can no longer sign in.";
    }

    private List<DetailInfo> GetDetails()
    {
        var details = new List<DetailInfo>();

        // The deleted user does not need to be told who they are, nor that they deleted their own account.
        if (!IsRecipientDeletedUser)
        {
            AddDetail("User", DeletedUserLabel);
        }

        AddDetail("Site", SiteName);

        if (!(IsRecipientDeletedUser && IsSelfDeletion))
        {
            AddDetail("Deleted by", DeletedByLabel);
        }

        AddDetail("Deleted at", DeletedAtLabel);

        return details;

        void AddDetail(string label, string value)
        {
            var detail = new DetailInfo
            {
                Label = label,
                Value = value
            };

            details.Add(detail);
        }
    }

    private static string BuildDetailsSection(IReadOnlyList<DetailInfo> details)
    {
        var section = new StringBuilder();

        foreach (var detail in details)
        {
            section.AppendLine($"{detail.Label + ":",-12}{detail.Value}");
        }

        return section.ToString();
    }
}
