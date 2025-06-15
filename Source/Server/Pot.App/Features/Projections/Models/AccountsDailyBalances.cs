namespace Pot.App.Features.Projections.Models;

public sealed class AccountsDailyBalances : Dictionary<Guid, List<DateBalance>>
{
    public AccountsDailyBalances(IDictionary<Guid, List<DateBalance>> dictionary)
        : base(dictionary)
    {
    }
}
