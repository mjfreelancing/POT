using Pot.EmailSender;
using Pot.RazorComponents.Models;

namespace Pot.AspNetCore.Integration.Tests.Host;

/// <summary>
/// Stands in for the channel writer in the integration host, keeping every message the application submits so
/// that a test can read it.
/// </summary>
/// <remarks>
/// The host removes every hosted service, so the real channel would never be consumed and its messages would be
/// lost. Capturing them is what makes an invitation reachable by a test at all, because its temporary password
/// exists nowhere else. This type lives in the test project by design: the shipped application has no way to
/// record a message.
/// </remarks>
public sealed class CapturingEmailWriter : ISendEmailChannelWriter
{
    /// <summary>
    /// A message the application submitted, with the type it was submitted as.
    /// </summary>
    /// <param name="Type">The message's type, as the application declared it.</param>
    /// <param name="Content">The message content, in the model the application built it from.</param>
    public sealed record CapturedEmail(EmailType Type, EmailConfigBase Content);

    private readonly List<CapturedEmail> _messages = [];
    private readonly Lock _gate = new();

    /// <inheritdoc />
    public ValueTask SubmitAsync(EmailType emailType, EmailConfigBase emailConfig, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _messages.Add(new CapturedEmail(emailType, emailConfig));
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Every message captured so far, in the order they were submitted.
    /// </summary>
    /// <returns>A snapshot that a later submission cannot change.</returns>
    public IReadOnlyList<CapturedEmail> Snapshot()
    {
        lock (_gate)
        {
            return [.. _messages];
        }
    }

    /// <summary>
    /// The most recent message of a type sent to an address.
    /// </summary>
    /// <param name="emailType">The type to match.</param>
    /// <param name="recipient">The address to match, compared case-insensitively.</param>
    /// <returns>The message, or <see langword="null" /> when nothing matched.</returns>
    public CapturedEmail? Latest(EmailType emailType, string recipient)
    {
        lock (_gate)
        {
            return _messages.LastOrDefault(message =>
                message.Type == emailType &&
                string.Equals(message.Content.Email, recipient, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Discards everything captured so far, for a test that only cares about what follows.
    /// </summary>
    public void Clear()
    {
        lock (_gate)
        {
            _messages.Clear();
        }
    }
}
