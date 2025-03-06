using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Import;

internal sealed class Request
{
    [Description("A CSV file containing the account details to be imported.")]
    public required IFormFile File { get; set; }
}