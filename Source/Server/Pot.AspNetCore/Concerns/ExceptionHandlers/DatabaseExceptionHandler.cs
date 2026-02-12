using AllOverIt.Assertion;
using AllOverIt.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pot.App.Errors;
using Pot.AspNetCore.Concerns.ProblemDetails;

namespace Pot.AspNetCore.Concerns.ExceptionHandlers;

// Note: Exception handlers are registered as a Singleton.
internal sealed class DatabaseExceptionHandler : IExceptionHandler
{
    // Will also catch UniqueConstraintException and ReferenceConstraintException
    private static readonly Type DbUpdateExceptionType = typeof(DbUpdateException);

    private readonly IProblemDetailsService _problemDetailsService;

    public DatabaseExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService.WhenNotNull();
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        if (exceptionType.IsDerivedFrom(DbUpdateExceptionType))
        {
            ProblemDetailsContext problemContext;

            if (exception.InnerException is PostgresException postgresException)
            {
                var errorDetail = new ApiDetailError(ErrorType.Server)
                {
                    ErrorCode = ErrorCodes.Database,
                    ErrorMessage = postgresException.MessageText
                };

                problemContext = ProblemDetailsContextFactory.Create(
                    httpContext,
                    StatusCodes.Status500InternalServerError,
                    postgresException.MessageText,
                    exception, [errorDetail]);
            }
            else
            {
                var errorDetail = new ApiDetailError(ErrorType.Server)
                {
                    ErrorCode = ErrorCodes.Database,
                    ErrorMessage = exception.Message
                };

                problemContext = ProblemDetailsContextFactory.Create(
                    httpContext,
                    StatusCodes.Status500InternalServerError,
                    exception.Message,
                    exception,
                    [errorDetail]);
            }

            return await _problemDetailsService.TryWriteAsync(problemContext);
        }

        return false;
    }
}
