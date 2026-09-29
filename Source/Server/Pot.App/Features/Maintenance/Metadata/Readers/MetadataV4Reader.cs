using AllOverIt.Serialization.Binary.Readers;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.App.Features.Maintenance.Metadata.Serializer;

namespace Pot.App.Features.Maintenance.Metadata.Readers;

/// <summary>
/// Reads maintenance package metadata written in version 4, the current package format.
/// </summary>
/// <remarks>
/// This is the only metadata reader used by the runtime import path.
/// </remarks>
internal sealed class MetadataV4Reader : EnrichedBinaryValueReader<MetadataV4>
{
    /// <inheritdoc />
    public override object ReadValue(IEnrichedBinaryReader reader)
    {
        // The version is read by the serializer using this reader.
        var createdAt = MetadataSerializationHelper.ReadCreatedAt(reader);

        return new MetadataV4
        {
            CreatedAt = createdAt
        };
    }
}
