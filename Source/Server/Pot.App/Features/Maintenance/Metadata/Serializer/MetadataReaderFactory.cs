using AllOverIt.Serialization.Binary.Readers;
using Pot.App.Features.Maintenance.Metadata.Readers;

namespace Pot.App.Features.Maintenance.Metadata.Serializer;

/// <summary>
/// Default implementation of <see cref="IMetadataReaderFactory"/>.
/// </summary>
internal sealed class MetadataReaderFactory : IMetadataReaderFactory
{
    /// <inheritdoc />
    public IEnrichedBinaryValueReader CreateReader() => new MetadataV4Reader();
}