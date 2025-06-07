using AllOverIt.Validation;
using AllOverIt.Validation.Extensions;
using Pot.App.Concerns.Validation;

namespace Pot.App.Extensions;

public static class LifetimeValidationRegistryExtensions
{
    private static readonly Type _scopedLifetimeValidatorType = typeof(IScopedLifetimeValidator);

    public static void AddAppValidators(this ILifetimeValidationRegistry validationRegistry)
    {
        validationRegistry.AutoRegisterScopedValidators<ValidationRegistrar>((modelType, validatorType) =>
        {
            return validatorType.IsAssignableTo(_scopedLifetimeValidatorType);
        });

        validationRegistry.AutoRegisterSingletonValidators<ValidationRegistrar>((modelType, validatorType) =>
        {
            // Validators are typically registered as singletons, so we look for the lack of IScopedLifetimeValidator.
            return !validatorType.IsAssignableTo(_scopedLifetimeValidatorType);
        });
    }
}