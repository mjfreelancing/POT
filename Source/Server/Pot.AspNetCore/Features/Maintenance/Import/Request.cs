using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Maintenance.Import;

internal sealed class Request
{
    [FromForm]
    [Description("A ZIP file containing the data to be imported.")]
    public required IFormFile File { get; set; }

    [FromHeader(Name = "Export-Public-Key")]
    public required string ExportPublicKey { get; set; }
}
