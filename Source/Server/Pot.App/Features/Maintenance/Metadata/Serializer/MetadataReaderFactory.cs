using AllOverIt.Serialization.Binary.Readers;
using Pot.App.Features.Maintenance.Metadata.Readers;
using System.Diagnostics;

namespace Pot.App.Features.Maintenance.Metadata.Serializer;

internal sealed class MetadataReaderFactory : IMetadataReaderFactory
{
    public IEnrichedBinaryValueReader CreateReader(int version)
    {
        return version switch
        {
            1 => new MetadataV1Reader(),
            _ => throw new UnreachableException($"Import metadata version {version} is not supported.")
        };
    }
}