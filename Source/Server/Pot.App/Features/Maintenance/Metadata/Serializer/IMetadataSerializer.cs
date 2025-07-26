using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Metadata.Serializer;

public interface IMetadataSerializer : IPotSingletonDependency
{
    byte[] Serialize<TMetadata>(TMetadata metadata) where TMetadata : MetadataBase;
    MetadataBase Deserialize(Stream zipStream);
}
