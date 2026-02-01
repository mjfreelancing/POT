namespace Pot.RazorComponents.Models;

[Flags]
public enum EmailFormatType
{
    Html = 1,
    PlainText = 2,
    Both = Html | PlainText
}
