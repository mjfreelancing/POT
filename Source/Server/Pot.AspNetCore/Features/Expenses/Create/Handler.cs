using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Expenses.Create;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Expenses.Create.Mappings;

namespace Pot.AspNetCore.Features.Expenses.Create;

internal sealed class Handler
{
    public static async Task<Results<CreatedAtRoute<Response>, ProblemHttpResult>> Invoke(Request request,
        ICreateExpenseService expenseService, IProblemDetailsInspector problemDetailsInspector,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var validationContext = new RequestValidationContext
        {
            NextDue = request.NextDue,
            EndDate = request.EndDate,
            Frequency = request.Frequency
        };

        var problemDetails = problemDetailsInspector.Validate(request, validationContext);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var expenseInput = request.MapToInput();

        var expenseOutput = await expenseService.CreateExpenseAsync(expenseInput, cancellationToken);

        return expenseOutput.IsSuccess
            ? Response.Created(expenseOutput.Value!)
            : TypedResults.Problem(expenseOutput.Error!.ToProblemDetails());
    }
}
