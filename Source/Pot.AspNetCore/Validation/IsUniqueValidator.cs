using AllOverIt.Extensions;
using FluentValidation;
using FluentValidation.Results;
using System.Linq.Expressions;

namespace Pot.AspNetCore.Validation;

// TODO: Move to AOI
public sealed class IsUniqueValidator<TType, TProperty> : AbstractValidator<IEnumerable<TType>>
{
    public IsUniqueValidator(Expression<Func<TType, TProperty>> propertySelector, IEqualityComparer<TProperty>? comparer = default)
    {
        RuleFor(collection => collection)
            .Custom((collection, context) =>
            {
                if (collection is null)
                {
                    return;
                }

                var (propertyName, func) = IsUniqueValidatorHelpers.GetPropertyNameAndFunc(propertySelector);

                var duplicateValues = IsUniqueValidatorHelpers.GetDuplicates(collection, func, comparer);

                if (duplicateValues.Length != 0)
                {
                    context.AddFailure(new ValidationFailure(
                        propertyName: propertyName,
                        errorMessage: $"'{propertyName}' must have unique values.",
                        attemptedValue: duplicateValues
                    ));
                }
            });
    }
}

public sealed class IsUniqueValidator<TType, TProperty1, TProperty2> : AbstractValidator<IEnumerable<TType>>
{
    public IsUniqueValidator(Expression<Func<TType, TProperty1>> property1Selector, Expression<Func<TType, TProperty2>> property2Selector,
        IEqualityComparer<(TProperty1, TProperty2)>? comparer = default)
    {
        RuleFor(collection => collection)
            .Custom((collection, context) =>
            {
                if (collection is null)
                {
                    return;
                }

                var (property1Name, func1) = IsUniqueValidatorHelpers.GetPropertyNameAndFunc(property1Selector);
                var (property2Name, func2) = IsUniqueValidatorHelpers.GetPropertyNameAndFunc(property2Selector);

                var duplicateValues = IsUniqueValidatorHelpers.GetDuplicates(collection, item => (func1.Invoke(item), func2.Invoke(item)), comparer);

                if (duplicateValues.Length != 0)
                {
                    foreach (var key in duplicateValues)
                    {
                        var values = new[] { key.Item1?.ToString() ?? string.Empty, key.Item2?.ToString() ?? string.Empty };

                        context.AddFailure(new ValidationFailure(
                            propertyName: $"{property1Name}, {property2Name}",
                            errorMessage: $"The combination of '{property1Name}' and '{property2Name}' must be unique.",
                            attemptedValue: string.Join(", ", values)
                        ));
                    }
                }
            });
    }
}

// Extend to more than two

internal static class IsUniqueValidatorHelpers
{
    public static (string, Func<TType, TProperty>) GetPropertyNameAndFunc<TType, TProperty>(Expression<Func<TType, TProperty>> propertySelector)
    {
        return (propertySelector.GetPropertyOrFieldMemberInfo().Name, propertySelector.Compile());
    }

    public static TProperty[] GetDuplicates<TType, TProperty>(IEnumerable<TType> collection, Func<TType, TProperty> selector, IEqualityComparer<TProperty>? comparer)
    {
        return [.. collection
          .GroupBy(selector.Invoke, comparer)
          .Where(group => group.Count() > 1)
          .Select(group => group.Key)];
    }
}