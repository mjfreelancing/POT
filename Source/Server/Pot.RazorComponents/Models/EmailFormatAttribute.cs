namespace Pot.RazorComponents.Models;

[AttributeUsage(AttributeTargets.Property)]
public sealed class EmailFormatAttribute : Attribute
{
    public EmailFormatType Format { get; }

    public EmailFormatAttribute(EmailFormatType format)
    {
        Format = format;
    }
}
