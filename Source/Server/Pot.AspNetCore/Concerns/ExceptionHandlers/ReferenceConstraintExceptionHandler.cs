using AllOverIt.Assertion;
using AllOverIt.Extensions;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Diagnostics;
using Pot.App.Errors;
using Pot.AspNetCore.Concerns.ProblemDetails;
using System.Net;

namespace Pot.AspNetCore.Concerns.ExceptionHandlers;

internal sealed class ReferenceConstraintExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public ReferenceConstraintExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService.WhenNotNull();
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ReferenceConstraintException constraintException)
        {
            return false;
        }

        // constraintException.ConstraintProperties may include FK that we cannot report (since it is just an Id),
        // so we'll grab all values but exclude anything we don't have a value for.
        //
        // constraintException.Entries typically has a single entry, but in some cases it may be zero or multiple entries.
        // For this app, there will only ever be a single entry.
        var properties = constraintException.Entries.Single().Entity.ToPropertyDictionary();

        var propertyValues = properties
            .Where(kvp => constraintException.ConstraintProperties.Contains(kvp.Key))
            .ToDictionary();

        var propertiesLabel = propertyValues.Keys.Count > 1 ? "properties" : "property";
        var constraintNames = string.Join(", ", propertyValues.Keys);
        var detail = $"A constraint violation occurred with the {propertiesLabel} {constraintNames}";

        var errorDetail = new ProblemDetailsError(ProblemType.Conflict)
        {
            ErrorCode = ErrorCodes.Conflict,
            PropertyName = constraintNames,
            AttemptedValue = string.Join(", ", propertyValues.Values.Select(value => value?.ToString() ?? "(null)")),
            ErrorMessage = detail
        };

        var problemContext = ProblemDetailsContextFactory.Create(httpContext, (int)HttpStatusCode.Conflict, detail, exception, [errorDetail]);

        return await _problemDetailsService.TryWriteAsync(problemContext);
    }
}

