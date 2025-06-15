namespace Pot.App.Features.Projections.Models;

public sealed class GlobalDailyBalances : Dictionary<DateOnly, double>
{
    public GlobalDailyBalances(IDictionary<DateOnly, double> dictionary)
        : base(dictionary)
    {
    }
}