namespace Pot.App.Features.Maintenance.Metadata.Models;

/// <summary>
/// Exported columns for metadata version 4.
///
/// Diff from previous version (v3):
/// - accounts.csv: removed Bsb and Number.
/// - incomes.csv: unchanged from v3.
/// - expenses.csv: unchanged from v3.
///
/// accounts.csv:
/// - RowId (Guid)
/// - Description (string)
/// - Balance (double)
/// - Reserved (double)
/// - TotalExpenseAccrued (double)
/// - DailyExpenseAccrual (double)
/// - StableExpenseAccrual (double)
///
/// incomes.csv:
/// - RowId (Guid)
/// - ExcludeFromCalcs (bool)
/// - Description (string)
/// - NextDue (DateOnly)
/// - EndDate (DateOnly?)
/// - Frequency (Frequency)
/// - FrequencyCount (int)
/// - Amount (double)
/// - Note (string?)
/// - AccountRowId (Guid)
///
/// expenses.csv:
/// - RowId (Guid)
/// - ExcludeFromCalcs (bool)
/// - Description (string)
/// - AccrualStart (DateOnly?)
/// - NextDue (DateOnly)
/// - EndDate (DateOnly?)
/// - AccrualPolicy (AccrualPolicy)
/// - Frequency (Frequency)
/// - FrequencyCount (int)
/// - Amount (double)
/// - Accrued (double)
/// - Note (string?)
/// - AccountRowId (Guid)
/// </summary>
internal sealed class MetadataV4 : MetadataBase
{
    public override int Version => 4;
}
