using FluentValidation;

namespace Pot.AspNetCore.Concerns.Validation.Extensions;

public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<TType, TProperty?> IsNullOrNonDefault<TType, TProperty>(this IRuleBuilder<TType, TProperty?> ruleBuilder)
        where TProperty : struct
    {
        return ruleBuilder
            .Must(value => !value.HasValue || !EqualityComparer<TProperty>.Default.Equals(value.Value, default))
            .WithErrorCode(ErrorCodes.Invalid)
            .WithMessage("Cannot be a default value.");
    }
}
