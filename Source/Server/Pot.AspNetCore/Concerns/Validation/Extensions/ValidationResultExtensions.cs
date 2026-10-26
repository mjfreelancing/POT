using FluentValidation.Results;
using Pot.AspNetCore.Concerns.ProblemDetails;

namespace Pot.AspNetCore.Concerns.Validation.Extensions;

internal static class ValidationResultExtensions
{
    /// <summary>
    /// Describes a failed validation as a validation problem.
    /// </summary>
    /// <param name="validationResult">The failures to report.</param>
    /// <returns>The problem body, carrying the messages in the <c>errors</c> map and no <c>detail</c>.</returns>
    public static Microsoft.AspNetCore.Mvc.ProblemDetails ToProblemDetails(this ValidationResult validationResult)
    {
        var failures = validationResult.Errors
            .Select(failure => new ValidationErrors.FieldFailure(failure.PropertyName, failure.ErrorMessage));

        return new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = ValidationErrors.Title,
            Extensions = new Dictionary<string, object?>
            {
                { ValidationErrors.Extension, ValidationErrors.ToMap(failures) }
            }
        };
    }
}
