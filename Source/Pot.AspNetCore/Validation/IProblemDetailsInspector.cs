namespace Pot.AspNetCore.Validation;

public interface IProblemDetailsInspector
{
    Microsoft.AspNetCore.Mvc.ProblemDetails Validate<TType>(TType instance);
    Task<Microsoft.AspNetCore.Mvc.ProblemDetails> ValidateAsync<TType>(TType instance, CancellationToken cancellationToken);
}
