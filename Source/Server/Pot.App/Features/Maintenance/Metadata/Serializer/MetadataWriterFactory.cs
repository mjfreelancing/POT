using AllOverIt.Serialization.Binary.Writers;
using Pot.App.Features.Maintenance.Metadata.Writers;

namespace Pot.App.Features.Maintenance.Metadata.Serializer;

internal sealed class MetadataWriterFactory : IMetadataWriterFactory
{
    public IEnrichedBinaryValueWriter CreateWriter() => new MetadataV3Writer();
}
