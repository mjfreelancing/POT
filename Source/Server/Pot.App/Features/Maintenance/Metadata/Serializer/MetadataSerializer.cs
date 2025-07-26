using AllOverIt.Assertion;
using AllOverIt.Serialization.Binary.Readers;
using AllOverIt.Serialization.Binary.Writers;
using Pot.App.Features.Maintenance.Metadata.Models;
using System.Text;

namespace Pot.App.Features.Maintenance.Metadata.Serializer;

internal sealed class MetadataSerializer : IMetadataSerializer
{
    private readonly IMetadataWriterFactory _metadataWriterFactory;
    private readonly IMetadataReaderFactory _metadataReaderFactory;

    public MetadataSerializer(IMetadataWriterFactory metadataWriterFactory, IMetadataReaderFactory metadataReaderFactory)
    {
        _metadataWriterFactory = metadataWriterFactory.WhenNotNull();
        _metadataReaderFactory = metadataReaderFactory.WhenNotNull();
    }

    public byte[] Serialize<TMetadata>(TMetadata metadata) where TMetadata : MetadataBase
    {
        using var stream = new MemoryStream();

        using (var writer = new EnrichedBinaryWriter(stream, Encoding.UTF8, true))
        {
            // Explicitly write the version number first as the reader needs to know this before it can create an appropriate reader.
            writer.Write(metadata.Version);

            var metadataWriter = _metadataWriterFactory.CreateWriter(metadata.Version);

            writer.Writers.Add(metadataWriter);
            writer.WriteObject(metadata);
        }

        return stream.ToArray();
    }

    public MetadataBase Deserialize(Stream zipStream)
    {
        using var reader = new EnrichedBinaryReader(zipStream, Encoding.UTF8, true);

        var version = reader.ReadInt32();

        var metadataReader = _metadataReaderFactory.CreateReader(version);

        // Add a version specific reader
        reader.Readers.Add(metadataReader);

        return (MetadataBase)reader.ReadObject()!;
    }
}