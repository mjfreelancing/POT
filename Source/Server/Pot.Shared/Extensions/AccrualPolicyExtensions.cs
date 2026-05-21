using Pot.Shared.Enumerations;

namespace Pot.Shared.Extensions;

/// <summary>
/// Extension methods for <see cref="AccrualPolicy"/> that resolve canonical accrual dates.
/// </summary>
public static class AccrualPolicyExtensions
{
    /// <summary>
    /// Returns the canonical accrual start date for the given <see cref="AccrualPolicy"/>.
    /// </summary>
    /// <param name="accrualPolicy">The accrual policy that governs how the start date is resolved.</param>
    /// <param name="accrualStart">The persisted accrual start date, if any.</param>
    /// <param name="automaticDefaultStart">The fallback start date used when the policy is <see cref="AccrualPolicy.Automatic"/> and <paramref name="accrualStart"/> is <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="null"/> when the policy is <see cref="AccrualPolicy.None"/>; otherwise the explicit
    /// <paramref name="accrualStart"/>, or <paramref name="automaticDefaultStart"/> when <paramref name="accrualStart"/> is <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="accrualPolicy"/> is not a supported value.</exception>
    public static DateOnly? GetCanonicalAccrualStart(this AccrualPolicy accrualPolicy, DateOnly? accrualStart, DateOnly automaticDefaultStart)
    {
        // Canonicalization rule:
        // - None policy never accrues, so persisted accrual start must be null.
        // - Automatic policy keeps an explicit accrual start; fall back to the caller's default when missing.
        return accrualPolicy switch
        {
            var currentPolicy when currentPolicy == AccrualPolicy.None => null,
            var currentPolicy when currentPolicy == AccrualPolicy.Automatic => accrualStart ?? automaticDefaultStart,
            _ => throw new ArgumentOutOfRangeException(nameof(accrualPolicy), accrualPolicy, "Unsupported accrual policy.")
        };
    }
}