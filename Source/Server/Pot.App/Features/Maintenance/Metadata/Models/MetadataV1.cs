namespace Pot.App.Features.Maintenance.Metadata.Models;

/// <summary>
/// Exported columns for metadata version 1.
///
/// Note: v1 changed over time before v2 was introduced.
///
/// Initial v1 schema (introduced with metadata v1):
/// accounts.csv:
/// - RowId (Guid)
/// - Bsb (string)
/// - Number (string)
/// - Description (string)
/// - Balance (double)
/// - Reserved (double)
/// - TotalExpenseAccrued (double)
/// - DailyExpenseAccrual (double)
///
/// incomes.csv:
/// - RowId (Guid)
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
/// - Description (string)
/// - AccrualStart (DateOnly)
/// - NextDue (DateOnly)
/// - EndDate (DateOnly?)
/// - Frequency (Frequency)
/// - FrequencyCount (int)
/// - Amount (double)
/// - Accrued (double)
/// - Note (string?)
/// - AccountRowId (Guid)
///
/// Late v1 schema (immediately before v2):
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
/// - Frequency (Frequency)
/// - FrequencyCount (int)
/// - Amount (double)
/// - Accrued (double)
/// - AccruedIsDirty (bool)
/// - LastAccruedUpdate (DateOnly?)
/// - Note (string?)
/// - AccountRowId (Guid)
/// </summary>
internal sealed class MetadataV1 : MetadataBase
{
    public override int Version => 1;
}
