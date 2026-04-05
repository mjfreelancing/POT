using AllOverIt.Serialization.Binary.Writers;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.App.Features.Maintenance.Metadata.Serializer;

namespace Pot.App.Features.Maintenance.Metadata.Writers;

internal sealed class MetadataV2Writer : EnrichedBinaryValueWriter<MetadataV2>
{
    public override void WriteValue(IEnrichedBinaryWriter writer, object value)
    {
        var metadata = (MetadataV2)value;

        // The version is written by the serializer using this writer.
        MetadataSerializationHelper.WriteCreatedAt(writer, metadata);
    }
}
