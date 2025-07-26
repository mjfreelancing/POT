using AllOverIt.Serialization.Binary.Writers;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Metadata.Serializer;

public interface IMetadataWriterFactory : IPotSingletonDependency
{
    IEnrichedBinaryValueWriter CreateWriter(int version);
}
