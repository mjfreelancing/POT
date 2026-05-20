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
            if (metadata.Version != MetadataBase.CurrentVersion)
            {
                throw new InvalidDataException($"Unexpected metadata version: {metadata.Version}");
            }

            // Explicitly write the version number first as the reader needs to verify this before creating the reader.
            writer.Write(metadata.Version);

            // We always create the most recent writer
            var metadataWriter = _metadataWriterFactory.CreateWriter();

            writer.Writers.Add(metadataWriter);
            writer.WriteObject(metadata);
        }

        return stream.ToArray();
    }

    public TMetadata Deserialize<TMetadata>(Stream zipStream) where TMetadata : MetadataBase
    {
        using var reader = new EnrichedBinaryReader(zipStream, Encoding.UTF8, true);

        // We need to read past the version number. Assuming it is the latest version.
        // Not validating the version number here. The import process will handle this.
        _ = reader.ReadInt32();

        // We only support the most recent version for reading.
        var metadataReader = _metadataReaderFactory.CreateReader();

        // Add a version specific reader
        reader.Readers.Add(metadataReader);

        return (TMetadata)reader.ReadObject()!;
    }
}