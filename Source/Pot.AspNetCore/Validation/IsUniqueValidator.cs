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

                var propertyName = propertySelector.GetPropertyOrFieldMemberInfo().Name;
                var func = propertySelector.Compile();

                var duplicateValues = collection
                    .Select(func)
                    .GroupBy(value => value, comparer)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .ToArray();

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
