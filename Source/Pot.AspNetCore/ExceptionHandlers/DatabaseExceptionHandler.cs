using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pot.AspNetCore.ProblemDetails;
using System.Net;

namespace Pot.AspNetCore.ExceptionHandlers;

// Note: Exception handlers are registered as a Singleton.
internal sealed class DatabaseExceptionHandler : IExceptionHandler
{
    private static readonly Type[] _databaseExceptionTypes = [typeof(DbUpdateException)];

    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILoggerFactory _loggerFactory;

    public DatabaseExceptionHandler(IProblemDetailsService problemDetailsService, ILoggerFactory loggerFactory)
    {
        _problemDetailsService = problemDetailsService.WhenNotNull();
        _loggerFactory = loggerFactory.WhenNotNull();
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        var isDatabaseException = _databaseExceptionTypes.Any(item => exceptionType == item);

        if (isDatabaseException)
        {
            _loggerFactory
                .CreateLogger<DatabaseExceptionHandler>()
                .LogAllExceptions(exception, null);


            var postgresException = (exception.InnerException as PostgresException)!;

            var errorDetails = new[]
            {
                new PostgresProblemDetails
                {
                    ErrorMessage = postgresException.MessageText,
                    SqlState = postgresException.SqlState
                }
            };

            var problemContext = ProblemDetailsContextFactory.Create(httpContext, exception, (int)HttpStatusCode.UnprocessableEntity, errorDetails);

            return await _problemDetailsService.TryWriteAsync(problemContext);
        }

        return isDatabaseException;
    }
}
