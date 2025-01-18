using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Pot.AspNetCore.ProblemDetails;
using Pot.Data.Exceptions;
using System.Net;

namespace Pot.AspNetCore.ExceptionHandlers;

// Note: Exception handlers are registered as a Singleton.
internal sealed class EntityExceptionHandler : IExceptionHandler
{
    // Such as: EntityNotFoundException<>, EntityConflictException<>
    private static readonly Type _databaseExceptionType = typeof(DatabaseException);

    private readonly IProblemDetailsService _problemDetailsService;

    public EntityExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService.WhenNotNull();
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        var isDatabaseException = exceptionType.IsDerivedFrom(_databaseExceptionType);

        if (isDatabaseException)
        {
            var databaseException = exception as DatabaseException;

            var errorDetails = new[]
            {
                new EntityProblemDetails
                {
                    ErrorMessage = exception.Message,
                    EntityType = databaseException!.EntityType.Name
                }
            };

            var problemContext = ProblemDetailsContextFactory.Create(httpContext, exception, (int)HttpStatusCode.UnprocessableEntity, errorDetails);

            return await _problemDetailsService.TryWriteAsync(problemContext);
        }

        return isDatabaseException;
    }
}

