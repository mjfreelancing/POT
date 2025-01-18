namespace Pot.AspNetCore.Validation;

public interface IProblemDetailsInspector
{
    Microsoft.AspNetCore.Mvc.ProblemDetails Validate<TType>(TType instance);
}
