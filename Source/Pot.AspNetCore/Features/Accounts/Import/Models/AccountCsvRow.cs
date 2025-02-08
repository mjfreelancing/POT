using CsvHelper.Configuration.Attributes;

namespace Pot.AspNetCore.Features.Accounts.Import.Models;

public sealed class AccountCsvRow
{
    // If an Id is not provided then a new GUID will be assigned
    [Index(0)]
    public Guid? Id { get; init; }

    [Index(1)]
    public string Bsb { get; init; } = string.Empty;

    [Index(2)]
    public string Number { get; init; } = string.Empty;

    [Index(3)]
    public string Description { get; init; } = string.Empty;

    [Index(4)]
    public double Balance { get; init; }

    [Index(5)]
    public double Reserved { get; init; }

    // Allocated and DailyAccrual are calculated based on linked expenses so
    // we should not import these values as there are no associated expenses.
}
