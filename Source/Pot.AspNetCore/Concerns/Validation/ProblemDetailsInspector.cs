using AllOverIt.Assertion;
using AllOverIt.Validation;
using FluentValidation.Results;
using Pot.AspNetCore.Concerns.ProblemDetails;
using Pot.AspNetCore.Concerns.Validation.Extensions;

namespace Pot.AspNetCore.Concerns.Validation;

internal sealed class ProblemDetailsInspector : IProblemDetailsInspector
{
    private sealed class For<TType>;

    private readonly ILifetimeValidationInvoker _validationInvoker;

    public ProblemDetailsInspector(ILifetimeValidationInvoker validationInvoker)
    {
        _validationInvoker = validationInvoker.WhenNotNull();
    }

    public static Microsoft.AspNetCore.Mvc.ProblemDetails ValidateAsRowId(string id, string entityType, out Guid entityId)
    {
        return Guid.TryParse(id, out entityId)
            ? NoProblemDetails.Single
            : ProblemDetailsFactory.CreateUnprocessableEntity(nameof(id), id, $"The {entityType} Id is invalid.");
    }

    public Microsoft.AspNetCore.Mvc.ProblemDetails Validate<TType>(TType instance)
    {
        var validationResult = _validationInvoker.Validate(instance);

        return AsProblemDetails(validationResult);
    }

    public async Task<Microsoft.AspNetCore.Mvc.ProblemDetails> ValidateAsync<TType>(TType instance, CancellationToken cancellationToken)
    {
        var validationResult = await _validationInvoker.ValidateAsync(instance, cancellationToken);

        return AsProblemDetails(validationResult);
    }

    private static Microsoft.AspNetCore.Mvc.ProblemDetails AsProblemDetails(ValidationResult validationResult)
    {
        return validationResult.IsValid
            ? NoProblemDetails.Single
            : validationResult.ToProblemDetails();
    }
}
