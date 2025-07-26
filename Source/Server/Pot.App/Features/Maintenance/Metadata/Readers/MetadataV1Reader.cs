using AllOverIt.Serialization.Binary.Readers;
using AllOverIt.Serialization.Binary.Readers.Extensions;
using Pot.App.Features.Maintenance.Metadata.Models;

namespace Pot.App.Features.Maintenance.Metadata.Readers;

internal sealed class MetadataV1Reader : EnrichedBinaryValueReader<MetadataV1>
{
    public override object ReadValue(IEnrichedBinaryReader reader)
    {
        // The version is read by the serializer using this reader
        var createdAt = reader.ReadDateTime();

        return new MetadataV1
        {
            CreatedAt = createdAt
        };
    }
}