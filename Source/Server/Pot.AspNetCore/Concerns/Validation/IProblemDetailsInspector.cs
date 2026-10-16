namespace Pot.AspNetCore.Concerns.Validation;

/// <summary>Inspects validation results and represents failures as problem details.</summary>
public interface IProblemDetailsInspector
{
    /// <summary>Validates the supplied value and returns the validation result.</summary>
    /// <param name="instance">The object to validate.</param>
    Microsoft.AspNetCore.Mvc.ProblemDetails Validate<TType>(TType instance);
    /// <summary>Validates the supplied value and returns the validation result.</summary>
    /// <param name="instance">The object to validate.</param>
    /// <param name="context">The validation context used when inspecting the value.</param>
    Microsoft.AspNetCore.Mvc.ProblemDetails Validate<TType, TContext>(TType instance, TContext context);
    /// <summary>Validates the supplied value asynchronously and describes any validation failures.</summary>
    /// <param name="instance">The object to validate.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    Task<Microsoft.AspNetCore.Mvc.ProblemDetails> ValidateAsync<TType>(TType instance, CancellationToken cancellationToken);
}
