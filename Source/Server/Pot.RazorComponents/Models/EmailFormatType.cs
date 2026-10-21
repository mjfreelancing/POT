namespace Pot.RazorComponents.Models;

[Flags]
public enum EmailFormatType
{
    /// <summary>Not passed to either template. Used for raw values that a formatted property is composed from.</summary>
    None = 0,

    Html = 1,
    PlainText = 2,
    Both = Html | PlainText
}
