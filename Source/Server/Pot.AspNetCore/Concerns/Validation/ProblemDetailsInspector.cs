using AllOverIt.Assertion;
using AllOverIt.Validation;
using FluentValidation.Results;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.Validation.Extensions;

namespace Pot.AspNetCore.Concerns.Validation;

internal sealed class ProblemDetailsInspector : IProblemDetailsInspector
{
    private readonly ILifetimeValidationInvoker _validationInvoker;
    private readonly ILogger<ProblemDetailsInspector> _logger;

    public ProblemDetailsInspector(ILifetimeValidationInvoker validationInvoker, ILogger<ProblemDetailsInspector> logger)
    {
        _validationInvoker = validationInvoker.WhenNotNull();
        _logger = logger.WhenNotNull();
    }

    public Microsoft.AspNetCore.Mvc.ProblemDetails Validate<TType>(TType instance)
    {
        var validationResult = _validationInvoker.Validate(instance);

        return AsProblemDetails(validationResult);
    }

    public Microsoft.AspNetCore.Mvc.ProblemDetails Validate<TType, TContext>(TType instance, TContext context)
    {
        var validationResult = _validationInvoker.Validate(instance, context);

        return AsProblemDetails(validationResult);
    }

    public async Task<Microsoft.AspNetCore.Mvc.ProblemDetails> ValidateAsync<TType>(TType instance, CancellationToken cancellationToken)
    {
        var validationResult = await _validationInvoker.ValidateAsync(instance, cancellationToken);

        return AsProblemDetails(validationResult);
    }

    private Microsoft.AspNetCore.Mvc.ProblemDetails AsProblemDetails(ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            return NoProblemDetails.Single;
        }

        Log(validationResult);

        return validationResult.ToProblemDetails();
    }

    // Logging happens once, here: a handler that logged the same failure would record every rejection twice. Only
    // the field name and the message are written - never the value the caller sent.
    private void Log(ValidationResult validationResult)
    {
        foreach (var failure in validationResult.Errors)
        {
            _logger.LogInformation("Validation Error: {ValidationErrorMessage} (Property: {Property})",
                failure.ErrorMessage, failure.PropertyName);
        }
    }
}
