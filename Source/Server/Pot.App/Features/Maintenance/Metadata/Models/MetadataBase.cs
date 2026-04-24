namespace Pot.App.Features.Maintenance.Metadata.Models;

/// <summary>
/// Metadata versions for maintenance import/export packages.
///
/// Package structure (all versions):
/// - metadata.bin
/// - accounts.csv
/// - incomes.csv
/// - expenses.csv
///
/// Column-level export documentation is version-specific and maintained in:
/// - MetadataV1 (includes initial v1 and late-v1 schema variants)
/// - MetadataV2
/// - MetadataV3
/// </summary>
public abstract class MetadataBase
{
    public const int CurrentVersion = 3;

    public abstract int Version { get; }
    public required DateTime CreatedAt { get; init; }
}
