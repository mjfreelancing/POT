using FluentValidation;
using System.Linq.Expressions;

namespace Pot.AspNetCore.Validation.Extensions;

// TODO: Move to AOI
public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<TType, IEnumerable<TElement>> IsUnique<TType, TElement, TProperty>(
        this IRuleBuilder<TType, IEnumerable<TElement>> ruleBuilder, Expression<Func<TElement, TProperty>> propertySelector,
        IEqualityComparer<TProperty>? comparer = default)
    {
        return ruleBuilder.SetValidator(new IsUniqueValidator<TElement, TProperty>(propertySelector, comparer));
    }
}
