using System.ComponentModel;

namespace Pot.AspNetCore.Features.Accounts.Import;

internal sealed class Request
{
    [Description("A CSV file containing the account details to be imported.")]
    public required IFormFile File { get; set; }

    [Description("When true, will overwrite the details of accounts that already exist, otherwise they will be skipped.")]
    public bool Overwrite { get; set; }
}