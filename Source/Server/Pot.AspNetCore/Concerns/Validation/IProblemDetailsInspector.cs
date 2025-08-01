namespace Pot.AspNetCore.Concerns.Validation;

public interface IProblemDetailsInspector
{
    Microsoft.AspNetCore.Mvc.ProblemDetails Validate<TType>(TType instance);
    Microsoft.AspNetCore.Mvc.ProblemDetails Validate<TType, TContext>(TType instance, TContext context);
    Task<Microsoft.AspNetCore.Mvc.ProblemDetails> ValidateAsync<TType>(TType instance, CancellationToken cancellationToken);
}
