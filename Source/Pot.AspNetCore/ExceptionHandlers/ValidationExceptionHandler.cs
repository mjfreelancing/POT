using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Logging.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Pot.AspNetCore.ProblemDetails;
using System.Net;

namespace Pot.AspNetCore.ExceptionHandlers;

// Note: Exception handlers are registered as a Singleton.
internal sealed class ValidationExceptionHandler : IExceptionHandler
{
    private static readonly Type _validationExceptionType = typeof(ValidationException);

    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILoggerFactory _loggerFactory;

    public ValidationExceptionHandler(IProblemDetailsService problemDetailsService, ILoggerFactory loggerFactory)
    {
        _problemDetailsService = problemDetailsService.WhenNotNull();
        _loggerFactory = loggerFactory.WhenNotNull();
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        var isValidationException = exceptionType == _validationExceptionType;

        if (isValidationException)
        {
            _loggerFactory
                .CreateLogger<ValidationExceptionHandler>()
                .LogAllExceptions(exception, null);

            var validationException = exception as ValidationException;

            var errorDetails = CreateValidationFailures(validationException!);

            var problemContext = ProblemDetailsContextFactory.Create(httpContext, exception, (int)HttpStatusCode.UnprocessableEntity, errorDetails);

            return await _problemDetailsService.TryWriteAsync(problemContext);
        }

        return isValidationException;
    }

    private static ProblemDetailsExtension[] CreateValidationFailures(ValidationException exception)
    {
        var errors = exception!.Errors;

        return errors.SelectToArray(error => new ValidationProblemDetails
        {
            Property = error.PropertyName,
            ErrorCode = error.ErrorCode,
            AttemptedValue = error.AttemptedValue,
            ErrorMessage = error.ErrorMessage
        });
    }
}
