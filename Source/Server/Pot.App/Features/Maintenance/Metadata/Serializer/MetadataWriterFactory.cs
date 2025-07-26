using AllOverIt.Serialization.Binary.Writers;
using Pot.App.Features.Maintenance.Metadata.Writers;
using System.Diagnostics;

namespace Pot.App.Features.Maintenance.Metadata.Serializer;

internal sealed class MetadataWriterFactory : IMetadataWriterFactory
{
    public IEnrichedBinaryValueWriter CreateWriter(int version)
    {
        return version switch
        {
            1 => new MetadataV1Writer(),
            _ => throw new UnreachableException($"Export metadata version {version} is not supported.")
        };
    }
}
