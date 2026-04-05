using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Expenses.Update;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Expenses.Update.Mappings;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Expenses.Update;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, NotFound, ProblemHttpResult>> Invoke([Description("The expense Id")] Guid id,
        Request request, IUpdateExpenseService expenseService, IProblemDetailsInspector problemDetailsInspector,
        ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var validationContext = new RequestValidationContext
        {
            NextDue = request.NextDue,
            EndDate = request.EndDate,
            AccrualPolicy = request.AccrualPolicy,
            Frequency = request.Frequency
        };

        var problemDetails = problemDetailsInspector.Validate(request, validationContext);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var expenseInput = request.MapToInput(id);

        var result = await expenseService.UpdateExpenseAsync(expenseInput, cancellationToken);

        return result.IsSuccess
            ? Response.Ok(result.Value!)
            : TypedResults.Problem(result.Error!.ToProblemDetails());
    }
}
