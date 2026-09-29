using AllOverIt.Serialization.Binary.Writers;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.App.Features.Maintenance.Metadata.Serializer;

namespace Pot.App.Features.Maintenance.Metadata.Writers;

/// <summary>
/// Writes maintenance package metadata in version 3 of the package format.
/// </summary>
internal sealed class MetadataV3Writer : EnrichedBinaryValueWriter<MetadataV3>
{
    /// <inheritdoc />
    public override void WriteValue(IEnrichedBinaryWriter writer, object value)
    {
        var metadata = (MetadataV3)value;

        // The version is written by the serializer using this writer.
        MetadataSerializationHelper.WriteCreatedAt(writer, metadata);
    }
}
