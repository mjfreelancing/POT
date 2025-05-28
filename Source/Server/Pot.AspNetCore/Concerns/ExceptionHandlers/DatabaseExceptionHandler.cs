using AllOverIt.Assertion;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.Validation;
using System.Net;

namespace Pot.AspNetCore.Concerns.ExceptionHandlers;

// Note: Exception handlers are registered as a Singleton.
internal sealed class DatabaseExceptionHandler : IExceptionHandler
{
    // UniqueConstraintException and ReferenceConstraintException are explicitly handled. This handler is for unexpected cases.
    private static readonly Type _dbUpdateExceptionType = typeof(DbUpdateException);

    private readonly IProblemDetailsService _problemDetailsService;

    public DatabaseExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService.WhenNotNull();
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        if (exceptionType == _dbUpdateExceptionType && exception.InnerException is PostgresException postgresException)
        {
            var errorDetail = new ProblemDetailsError
            {
                PropertyName = string.Empty,
                ErrorCode = ErrorCodes.IO,
                AttemptedValue = null,
                ErrorMessage = postgresException.Detail ?? postgresException.MessageText
            };

            var problemContext = ProblemDetailsContextFactory.Create(httpContext, (int)HttpStatusCode.Conflict, postgresException.MessageText, exception, [errorDetail]);

            return await _problemDetailsService.TryWriteAsync(problemContext);
        }

        return false;
    }
}
