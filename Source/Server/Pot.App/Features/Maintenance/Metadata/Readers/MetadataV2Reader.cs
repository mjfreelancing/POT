using AllOverIt.Serialization.Binary.Readers;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.App.Features.Maintenance.Metadata.Serializer;

namespace Pot.App.Features.Maintenance.Metadata.Readers;

/// <summary>
/// Reads maintenance package metadata written in version 2 of the package format.
/// </summary>
/// <remarks>
/// Superseded by version 4; version 2 metadata is not read by the runtime import path.
/// </remarks>
internal sealed class MetadataV2Reader : EnrichedBinaryValueReader<MetadataV2>
{
    /// <inheritdoc />
    public override object ReadValue(IEnrichedBinaryReader reader)
    {
        // The version is read by the serializer using this reader.
        var createdAt = MetadataSerializationHelper.ReadCreatedAt(reader);

        return new MetadataV2
        {
            CreatedAt = createdAt
        };
    }
}
