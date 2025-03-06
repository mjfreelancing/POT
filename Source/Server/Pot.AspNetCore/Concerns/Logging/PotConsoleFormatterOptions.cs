using Microsoft.Extensions.Logging.Console;

namespace Pot.AspNetCore.Concerns.Logging;

internal sealed class PotConsoleFormatterOptions : ConsoleFormatterOptions
{
    /// <summary>Gets or sets a value that indicates whether the entire message is logged in a single line.</summary>
    /// <value><see langword="true" /> if the entire message is logged in a single line.</value>
    public bool SingleLine { get; set; }
}
