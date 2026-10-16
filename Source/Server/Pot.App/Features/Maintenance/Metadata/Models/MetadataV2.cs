namespace Pot.App.Features.Maintenance.Metadata.Models;

/// <summary>
/// Exported columns for metadata version 2.
///
/// Diff from previous version (late v1):
/// - accounts.csv: unchanged from late v1.
/// - incomes.csv: unchanged from late v1.
/// - expenses.csv: added AccrualPolicy after EndDate.
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
/// - AccruedIsDirty (bool)
/// - LastAccruedUpdate (DateOnly?)
/// - Note (string?)
/// - AccountRowId (Guid)
/// </summary>
internal sealed class MetadataV2 : MetadataBase
{
    public override int Version => 2;
}
