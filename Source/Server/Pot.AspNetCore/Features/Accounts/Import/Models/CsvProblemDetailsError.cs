using Pot.AspNetCore.Concerns.ProblemDetails;

namespace Pot.AspNetCore.Features.Accounts.Import.Models;

internal sealed class CsvProblemDetailsError : ProblemDetailsError
{
    public int ImportRow { get; init; }
}
