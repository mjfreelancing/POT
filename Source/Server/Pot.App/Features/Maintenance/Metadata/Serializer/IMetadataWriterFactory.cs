using AllOverIt.Serialization.Binary.Writers;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Metadata.Serializer;

/// <summary>
/// Creates the value writer used to serialize the metadata entry of a maintenance export package.
/// </summary>
/// <remarks>
/// Only the current metadata version is written, so the factory always returns the writer for that version.
/// </remarks>
public interface IMetadataWriterFactory : IPotSingletonDependency
{
    /// <summary>
    /// Creates a value writer for the current metadata version.
    /// </summary>
    /// <returns>A value writer for the current metadata version.</returns>
    IEnrichedBinaryValueWriter CreateWriter();
}
