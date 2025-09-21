using AllOverIt.Validation;

namespace Pot.AspNetCore.Concerns.Validation;

internal abstract class PotValidatorBase<TType> : ValidatorBase<TType>
{
    static PotValidatorBase()
    {
        DisablePropertyNameSplitting();
    }
}