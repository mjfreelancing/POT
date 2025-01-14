using FluentValidation;
using System.Linq.Expressions;

namespace Pot.AspNetCore.Validation.Extensions;

// TODO: Move to AOI
public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<TType, IEnumerable<TElement>> IsUnique<TType, TElement, TProperty1>(
        this IRuleBuilder<TType, IEnumerable<TElement>> ruleBuilder, Expression<Func<TElement, TProperty1>> propertySelector,
        IEqualityComparer<TProperty1>? comparer = default)
    {
        return ruleBuilder.SetValidator(new IsUniqueValidator<TElement, TProperty1>(propertySelector, comparer));
    }

    public static IRuleBuilderOptions<TType, IEnumerable<TElement>> IsUnique<TType, TElement, TProperty1, TProperty2>(
        this IRuleBuilder<TType, IEnumerable<TElement>> ruleBuilder, Expression<Func<TElement, TProperty1>> property1Selector,
        Expression<Func<TElement, TProperty2>> property2Selector, IEqualityComparer<(TProperty1, TProperty2)>? comparer = default)
    {
        return ruleBuilder.SetValidator(new IsUniqueValidator<TElement, TProperty1, TProperty2>(property1Selector, property2Selector, comparer));
    }

    // Extend to more than two
}
