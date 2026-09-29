namespace Pot.App.Features.Maintenance.Metadata.Models;

/// <summary>
/// Exported columns for metadata version 4.
///
/// Diff from previous version (v3):
/// - accounts.csv: removed Bsb and Number, and the three derived accrual columns.
/// - incomes.csv: unchanged from v3.
/// - expenses.csv: removed the derived Accrued column.
///
/// Why the accrual columns were removed without bumping the version: no v4 export was ever taken, so
/// no package exists in the field whose layout a reader could misread. That precondition is
/// load-bearing, because the removal is not trailing on the expenses side - Accrued sat between
/// Amount and Note, so removing it shifts Note and AccountRowId up one position, and a row model
/// that binds by position against an older file would move values between fields instead of
/// failing. The accounts side was genuinely trailing. Any future change to the column set, an
/// addition or a removal anywhere in the row, must bump the version instead.
///
/// accounts.csv:
/// - RowId (Guid)
/// - Description (string)
/// - Balance (double)
/// - Reserved (double)
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
/// - Note (string?)
/// - AccountRowId (Guid)
/// </summary>
internal sealed class MetadataV4 : MetadataBase
{
    public override int Version => 4;
}
