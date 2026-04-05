using AllOverIt.Serialization.Binary.Readers;
using AllOverIt.Serialization.Binary.Readers.Extensions;
using AllOverIt.Serialization.Binary.Writers;
using AllOverIt.Serialization.Binary.Writers.Extensions;
using Pot.App.Features.Maintenance.Metadata.Models;

namespace Pot.App.Features.Maintenance.Metadata.Serializer;

internal static class MetadataSerializationHelper
{
    public static DateTime ReadCreatedAt(IEnrichedBinaryReader reader)
    {
        return reader.ReadDateTime();
    }

    public static void WriteCreatedAt(IEnrichedBinaryWriter writer, MetadataBase metadata)
    {
        writer.WriteDateTime(metadata.CreatedAt);
    }
}
