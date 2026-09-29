using AllOverIt.Serialization.Binary.Writers;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.App.Features.Maintenance.Metadata.Serializer;

namespace Pot.App.Features.Maintenance.Metadata.Writers;

/// <summary>
/// Writes maintenance package metadata in version 1 of the package format.
/// </summary>
[Obsolete("Legacy metadata v1 writer retained for historical tracking and possible future preview tooling. Do not use in runtime import/export paths.", error: true)]
internal sealed class MetadataV1Writer : EnrichedBinaryValueWriter<MetadataV1>
{
    /// <inheritdoc />
    public override void WriteValue(IEnrichedBinaryWriter writer, object value)
    {
        var metadata = (MetadataV1)value;

        // The version is written by the serializer using this writer.
        MetadataSerializationHelper.WriteCreatedAt(writer, metadata);
    }
}
