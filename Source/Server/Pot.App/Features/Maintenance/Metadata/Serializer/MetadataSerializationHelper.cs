using AllOverIt.Serialization.Binary.Readers;
using AllOverIt.Serialization.Binary.Readers.Extensions;
using AllOverIt.Serialization.Binary.Writers;
using AllOverIt.Serialization.Binary.Writers.Extensions;
using Pot.App.Features.Maintenance.Metadata.Models;

namespace Pot.App.Features.Maintenance.Metadata.Serializer;

/// <summary>
/// Provides the shared read and write steps used by every metadata version reader and writer.
/// </summary>
internal static class MetadataSerializationHelper
{
    /// <summary>
    /// Reads the metadata creation timestamp.
    /// </summary>
    /// <param name="reader">The binary reader positioned at the creation timestamp.</param>
    /// <returns>The creation timestamp recorded in the metadata.</returns>
    public static DateTime ReadCreatedAt(IEnrichedBinaryReader reader)
    {
        return reader.ReadDateTime();
    }

    /// <summary>
    /// Writes the metadata creation timestamp.
    /// </summary>
    /// <param name="writer">The binary writer to write to.</param>
    /// <param name="metadata">The metadata whose creation timestamp is written.</param>
    public static void WriteCreatedAt(IEnrichedBinaryWriter writer, MetadataBase metadata)
    {
        writer.WriteDateTime(metadata.CreatedAt);
    }
}
