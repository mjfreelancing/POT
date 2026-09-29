using Pot.Shared.Enumerations;

namespace Pot.AspNetCore.Integration.Tests.Host.Models;

/// <summary>
/// Builds the income payloads the integration fixtures share, so each fixture states only what its scenario
/// actually varies.
/// </summary>
public static class IncomeRequestFactory
{
    /// <summary>
    /// Builds a request for an income, converting the dates and enumerations to their wire form.
    /// </summary>
    /// <param name="accountRowId">The account the income is credited to.</param>
    /// <param name="nextDue">When the income is next due.</param>
    /// <param name="frequency">How often the income recurs.</param>
    /// <param name="frequencyCount">The number of frequency units between occurrences.</param>
    /// <param name="amount">The income amount.</param>
    /// <returns>The income to send to the create or update endpoint.</returns>
    public static IncomeRequest CreateIncome(Guid accountRowId, DateOnly nextDue, Frequency frequency,
        int frequencyCount, double amount)
    {
        var request = new IncomeRequest
        {
            Description = $"Integration Income {Guid.NewGuid():N}",
            ExcludeFromCalcs = false,
            NextDue = nextDue.ToString("yyyy-MM-dd"),
            Frequency = frequency.Name,
            FrequencyCount = frequencyCount,
            Amount = amount,
            AccountRowId = accountRowId
        };

        return request;
    }
}
