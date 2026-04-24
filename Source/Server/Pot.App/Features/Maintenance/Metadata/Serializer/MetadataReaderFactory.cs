using AllOverIt.Serialization.Binary.Readers;
using Pot.App.Features.Maintenance.Metadata.Readers;

namespace Pot.App.Features.Maintenance.Metadata.Serializer;

internal sealed class MetadataReaderFactory : IMetadataReaderFactory
{
    public IEnrichedBinaryValueReader CreateReader() => new MetadataV3Reader();
}