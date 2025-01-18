namespace Pot.AspNetCore.Features.Accounts.Import.Models;

public sealed class AccountForImport
{
    public int Id { get; init; }
    public string Bsb { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public double Balance { get; init; }
    public double Reserved { get; init; }
    public double Allocated { get; init; }
    public double DailyAccrual { get; init; }
}
