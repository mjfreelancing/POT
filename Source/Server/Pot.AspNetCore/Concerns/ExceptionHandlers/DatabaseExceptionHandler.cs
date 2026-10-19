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
    private readonly ILogger<DatabaseExceptionHandler> _logger;

    public DatabaseExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<DatabaseExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        if (exceptionType.IsDerivedFrom(DbUpdateExceptionType))
        {
            // Since .NET 10 the exception handler middleware no longer logs an exception that an IExceptionHandler
            // reports as handled, so this is the only record of the failure.
            _logger.LogError(exception, "A database update failed while processing {RequestMethod} {RequestPath}: {ExceptionMessage}",
                httpContext.Request.Method, httpContext.Request.Path, exception.Message);

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
