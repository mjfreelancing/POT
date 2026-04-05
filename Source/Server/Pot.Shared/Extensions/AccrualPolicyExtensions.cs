using Pot.Shared.Enumerations;

namespace Pot.Shared.Extensions;

public static class AccrualPolicyExtensions
{
    // Canonicalization rule:
    // - None policy never accrues, so persisted accrual start must be null.
    // - Automatic policy keeps an explicit accrual start; fall back to the caller's default when missing.
    public static DateOnly? GetCanonicalAccrualStart(this AccrualPolicy accrualPolicy, DateOnly? accrualStart, DateOnly automaticDefaultStart)
    {
        return accrualPolicy switch
        {
            var currentPolicy when currentPolicy == AccrualPolicy.None => null,
            var currentPolicy when currentPolicy == AccrualPolicy.Automatic => accrualStart ?? automaticDefaultStart,
            _ => throw new ArgumentOutOfRangeException(nameof(accrualPolicy), accrualPolicy, "Unsupported accrual policy.")
        };
    }
}