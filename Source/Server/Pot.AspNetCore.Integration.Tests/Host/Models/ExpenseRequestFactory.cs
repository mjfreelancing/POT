using Pot.Shared.Enumerations;

namespace Pot.AspNetCore.Integration.Tests.Host.Models;

/// <summary>
/// Builds the expense payloads the integration fixtures share, so each fixture states only what its scenario
/// actually varies.
/// </summary>
public static class ExpenseRequestFactory
{
    /// <summary>
    /// Builds a request for a bill, converting the dates and enumerations to their wire form.
    /// </summary>
    /// <param name="accountRowId">The account the bill belongs to.</param>
    /// <param name="nextDue">When the bill is next due.</param>
    /// <param name="frequency">How often the bill recurs. Use <see cref="Frequency.OneTime" /> for a bill that does not recur.</param>
    /// <param name="frequencyCount">The number of frequency units between occurrences. Must be zero for a one-time bill.</param>
    /// <param name="amount">The bill amount.</param>
    /// <param name="accrualPolicy">The accrual policy. Must be <see cref="AccrualPolicy.None" /> when there is no accrual start.</param>
    /// <param name="accrualStart">When accrual begins, or <see langword="null" /> for a bill that does not accrue.</param>
    /// <returns>The expense to send to the create or update endpoint.</returns>
    public static ExpenseRequest CreateBill(Guid accountRowId, DateOnly nextDue, Frequency frequency, int frequencyCount,
        double amount, AccrualPolicy accrualPolicy, DateOnly? accrualStart)
    {
        var request = new ExpenseRequest
        {
            Description = $"Integration Bill {Guid.NewGuid():N}",
            ExcludeFromCalcs = false,
            AccrualStart = accrualStart?.ToString("yyyy-MM-dd"),
            NextDue = nextDue.ToString("yyyy-MM-dd"),
            AccrualPolicy = accrualPolicy.Name,
            Frequency = frequency.Name,
            FrequencyCount = frequencyCount,
            Amount = amount,
            AccountRowId = accountRowId
        };

        return request;
    }
}
