using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Maintenance.Import;

internal sealed class Request
{
    [FromForm]
    [Description("A ZIP file containing the data to be imported")]
    public required IFormFile File { get; set; }
}
