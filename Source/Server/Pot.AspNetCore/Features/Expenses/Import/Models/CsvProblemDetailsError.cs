using Pot.App.Errors;

namespace Pot.AspNetCore.Features.Expenses.Import.Models;

internal sealed class CsvProblemDetailsError : ProblemDetailsError
{
    public int ImportRow { get; init; }

    public CsvProblemDetailsError()
        : base(ProblemType.UnprocessableEntity)
    {
    }
}
