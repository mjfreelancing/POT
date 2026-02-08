using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Pot.App.Features.Settings.Update;
using Pot.AspNetCore.Concerns.Validation;
using Pot.AspNetCore.Extensions;
using Pot.AspNetCore.Features.Settings.Update.Mappings;
using System.ComponentModel;

namespace Pot.AspNetCore.Features.Settings.Update;

internal sealed class Handler
{
    public static async Task<Results<Ok<Response>, ProblemHttpResult>> Invoke([Description("The setting category")] string category,
        [Description("The setting key")] string key, Request request, IUpdateSettingService settingService,
        IProblemDetailsInspector problemDetailsInspector, ILogger<Handler> logger, CancellationToken cancellationToken)
    {
        logger.LogCall(null);

        var validationContext = new RequestValidationContext
        {
            Category = category,
            Key = key
        };

        var problemDetails = problemDetailsInspector.Validate(request, validationContext);

        if (problemDetails.IsProblem())
        {
            logger.LogErrors(problemDetails);

            return TypedResults.Problem(problemDetails);
        }

        var input = request.MapToInput(category, key);

        var result = await settingService.UpdateSettingAsync(input, cancellationToken);

        return result.IsSuccess
            ? Response.Ok(result.Value!)
            : TypedResults.Problem(result.Error!.ToProblemDetails());
    }
}
