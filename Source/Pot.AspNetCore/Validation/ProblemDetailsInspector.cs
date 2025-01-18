using AllOverIt.Assertion;
using AllOverIt.Validation;
using Pot.AspNetCore.ProblemDetails;
using Pot.AspNetCore.Validation.Extensions;

namespace Pot.AspNetCore.Validation;

internal sealed class ProblemDetailsInspector : IProblemDetailsInspector
{
    private sealed class For<TType>;

    private readonly ILifetimeValidationInvoker _validationInvoker;

    public ProblemDetailsInspector(ILifetimeValidationInvoker validationInvoker)
    {
        _validationInvoker = validationInvoker.WhenNotNull();
    }

    public Microsoft.AspNetCore.Mvc.ProblemDetails Validate<TType>(TType instance)
    {
        var validationResult = _validationInvoker.Validate(instance);

        if (validationResult.IsValid)
        {
            return NoProblemDetails.Single;
        }

        return validationResult.ToProblemDetails();
    }
}
