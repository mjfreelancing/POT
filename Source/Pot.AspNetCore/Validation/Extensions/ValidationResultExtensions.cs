﻿using AllOverIt.Extensions;
using FluentValidation.Results;
using Pot.AspNetCore.ProblemDetails;

namespace Pot.AspNetCore.Validation.Extensions;

internal static class ValidationResultExtensions
{
    public static Microsoft.AspNetCore.Mvc.ProblemDetails ToProblemDetails(this ValidationResult validationResult)
    {
        var errorDetails = validationResult.Errors.SelectToArray(error => new ProblemDetailsError
        {
            PropertyName = error.PropertyName,
            ErrorCode = error.ErrorCode,
            AttemptedValue = error.AttemptedValue,
            ErrorMessage = error.ErrorMessage
        });

        return ProblemDetailsFactory.CreateUnprocessableEntity(errorDetails);
    }
}

