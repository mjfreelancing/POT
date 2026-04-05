using AllOverIt.Serialization.Binary.Readers;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Metadata.Serializer;

public interface IMetadataReaderFactory : IPotSingletonDependency
{
    IEnrichedBinaryValueReader CreateReader();
}