using AllOverIt.Assertion;
using AllOverIt.Extensions;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
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

        var propertiesLabel = constraintException.ConstraintProperties.Count > 1 ? "properties" : "property";
        var constraintNames = string.Join(", ", constraintException.ConstraintProperties);

        var errorDetails = from entry in constraintException.Entries
                           let properties = entry.Entity.ToPropertyDictionary()
                           select new PostgresUniqueConstraintProblemDetails
                           {
                               ErrorMessage = $"A constraint violation occurred with the {propertiesLabel} {constraintNames}",
                               Properties = new Dictionary<string, object?>(
                                   from propertyName in properties.Keys
                                   where constraintException.ConstraintProperties.Contains(propertyName)
                                   select new KeyValuePair<string, object?>(propertyName, properties[propertyName]))
                           };

        var problemContext = ProblemDetailsContextFactory.Create(httpContext, exception, (int)HttpStatusCode.UnprocessableEntity, [.. errorDetails]);

        return await _problemDetailsService.TryWriteAsync(problemContext);
    }
}

