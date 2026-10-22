using AllOverIt.Assertion;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Pot.App.Errors;
using Pot.AspNetCore.Concerns.ProblemDetails;

namespace Pot.AspNetCore.Concerns.ExceptionHandlers;

// Note: Exception handlers are registered as a Singleton.
internal sealed class DatabaseExceptionHandler : IExceptionHandler
{
    // The response never carries the database message: it names tables, columns and constraints. The client gets a
    // generic message and the correlation id, which is enough to find the logged exception.
    internal const string ConflictMessage = "The request conflicts with the current state of the data.";
    internal const string DatabaseErrorMessage = "A database error occurred.";

    private sealed record DatabaseErrorMapping(Type ExceptionType, int StatusCode, string ErrorCode,
        string Message, LogLevel LogLevel);

    // The first mapping whose exception type matches is used, so a derived type must appear before its base type.
    // The conflicts are caused by the state of the data rather than a server fault - for example, two requests racing
    // past an "already exists" check, or a row changed or removed by another request after it was read.
    private static readonly DatabaseErrorMapping[] Mappings =
    [
        new(typeof(UniqueConstraintException), StatusCodes.Status409Conflict, ErrorCodes.Conflict, ConflictMessage, LogLevel.Warning),
        new(typeof(ReferenceConstraintException), StatusCodes.Status409Conflict, ErrorCodes.Conflict, ConflictMessage, LogLevel.Warning),
        new(typeof(DbUpdateConcurrencyException), StatusCodes.Status409Conflict, ErrorCodes.Conflict, ConflictMessage, LogLevel.Warning)
    ];

    // Every other DbUpdateException.
    private static readonly DatabaseErrorMapping FallbackMapping =
        new(typeof(DbUpdateException), StatusCodes.Status500InternalServerError, ErrorCodes.Database, DatabaseErrorMessage, LogLevel.Error);

    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<DatabaseExceptionHandler> _logger;

    public DatabaseExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<DatabaseExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateException)
        {
            return false;
        }

        var mapping = Array.Find(Mappings, candidate => candidate.ExceptionType.IsInstanceOfType(exception)) ?? FallbackMapping;

        // Since .NET 10 the exception handler middleware no longer logs an exception that an IExceptionHandler
        // reports as handled, so this is the only record of the failure.
        _logger.Log(mapping.LogLevel, exception,
            "A database update failed while processing {RequestMethod} {RequestPath}: {ExceptionMessage}",
            httpContext.Request.Method, httpContext.Request.Path, exception.Message);

        // The body states what happened and nothing about the database. The type separates a race or constraint
        // refusal from the conflicts an endpoint's own rules decide on.
        var problemType = mapping.StatusCode == StatusCodes.Status409Conflict
            ? ProblemTypes.ToConflictType(mapping.ErrorCode)
            : null;

        var problemContext = ProblemDetailsContextFactory.Create(httpContext, mapping.StatusCode, mapping.Message,
            exception, problemType);

        return await _problemDetailsService.TryWriteAsync(problemContext);
    }
}
