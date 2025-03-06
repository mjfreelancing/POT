using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Import;

internal sealed class Request
{
    [Description("A CSV file containing the expense details to be imported.")]
    public required IFormFile File { get; set; }
}