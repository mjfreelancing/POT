namespace Pot.App.Features.Maintenance.Metadata.Models;

/// <summary>
/// Exported columns for metadata version 3.
///
/// Diff from previous version (v2):
/// - accounts.csv: unchanged from v2.
/// - incomes.csv: unchanged from v2.
/// - expenses.csv: removed AccruedIsDirty and LastAccruedUpdate.
///
/// accounts.csv:
/// - RowId (Guid)
/// - Bsb (string)
/// - Number (string)
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
internal sealed class MetadataV3 : MetadataBase
{
    public override int Version => 3;
}
