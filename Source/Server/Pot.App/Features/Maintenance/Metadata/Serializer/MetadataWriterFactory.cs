using AllOverIt.Serialization.Binary.Writers;
using Pot.App.Features.Maintenance.Metadata.Writers;

namespace Pot.App.Features.Maintenance.Metadata.Serializer;

/// <summary>
/// Default implementation of <see cref="IMetadataWriterFactory"/>.
/// </summary>
internal sealed class MetadataWriterFactory : IMetadataWriterFactory
{
    /// <inheritdoc />
    public IEnrichedBinaryValueWriter CreateWriter() => new MetadataV4Writer();
}
