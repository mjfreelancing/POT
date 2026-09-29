using AllOverIt.Serialization.Binary.Readers;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Metadata.Serializer;

/// <summary>
/// Creates the value reader used to deserialize the metadata entry of a maintenance export package.
/// </summary>
/// <remarks>
/// Only the current metadata version is read, so the factory always returns the reader for that version rather than
/// selecting one based on the version stored in the package.
/// </remarks>
public interface IMetadataReaderFactory : IPotSingletonDependency
{
    /// <summary>
    /// Creates a value reader for the current metadata version.
    /// </summary>
    /// <returns>A value reader for the current metadata version.</returns>
    IEnrichedBinaryValueReader CreateReader();
}