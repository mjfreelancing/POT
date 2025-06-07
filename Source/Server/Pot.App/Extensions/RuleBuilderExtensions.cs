using FluentValidation;
using Pot.App.Errors;

namespace Pot.App.Extensions;

public static class RuleBuilderExtensions
{
    // Example, a Guid? can be null but cannot be 00000000-0000-0000-0000-000000000000
    public static IRuleBuilderOptions<TType, TProperty?> IsNullOrNonDefault<TType, TProperty>(this IRuleBuilder<TType, TProperty?> ruleBuilder)
        where TProperty : struct
    {
        return ruleBuilder
            .Must(value => !value.HasValue || !EqualityComparer<TProperty>.Default.Equals(value.Value, default))
            .WithErrorCode(ErrorCodes.Invalid)
            .WithMessage("Cannot be a default value.");
    }
}
