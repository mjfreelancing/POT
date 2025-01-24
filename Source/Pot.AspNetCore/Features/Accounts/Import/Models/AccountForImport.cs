namespace Pot.AspNetCore.Features.Accounts.Import.Models;

public sealed class AccountForImport
{
    public string Bsb { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public double Balance { get; init; }
    public double Reserved { get; init; }

    // Allocated and DailyAccrual are calculated based on linked expenses so
    // we should not import these values as there are no associated expenses,
}
