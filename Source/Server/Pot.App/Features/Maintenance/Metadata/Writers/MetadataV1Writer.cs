using AllOverIt.Serialization.Binary.Writers;
using AllOverIt.Serialization.Binary.Writers.Extensions;
using Pot.App.Features.Maintenance.Metadata.Models;

namespace Pot.App.Features.Maintenance.Metadata.Writers;

internal sealed class MetadataV1Writer : EnrichedBinaryValueWriter<MetadataV1>
{
    public override void WriteValue(IEnrichedBinaryWriter writer, object value)
    {
        var metadata = (MetadataV1)value;

        // The version is written by the serializer using this writer
        writer.WriteDateTime(metadata.CreatedAt);
    }
}
