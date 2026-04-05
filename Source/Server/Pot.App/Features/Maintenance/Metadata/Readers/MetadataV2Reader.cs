using AllOverIt.Serialization.Binary.Readers;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.App.Features.Maintenance.Metadata.Serializer;

namespace Pot.App.Features.Maintenance.Metadata.Readers;

internal sealed class MetadataV2Reader : EnrichedBinaryValueReader<MetadataV2>
{
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
