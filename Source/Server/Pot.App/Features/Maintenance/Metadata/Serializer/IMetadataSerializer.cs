using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Metadata.Serializer;

/// <summary>
/// Serializes and deserializes the metadata entry of a maintenance export package.
/// </summary>
/// <remarks>
/// The serialized form starts with the metadata version as a 32-bit integer, followed by the length-prefixed
/// assembly-qualified name of the metadata type and then its payload. Only the current version is written and read;
/// deserialization does not validate the version and casts the result to the requested type, so that type must match
/// the type named in the payload.
/// </remarks>
public interface IMetadataSerializer : IPotSingletonDependency
{
    /// <summary>
    /// Serializes metadata for an export package.
    /// </summary>
    /// <typeparam name="TMetadata">The metadata type to serialize.</typeparam>
    /// <param name="metadata">The metadata to serialize.</param>
    /// <returns>The serialized metadata bytes.</returns>
    /// <exception cref="InvalidDataException">
    /// Thrown when <paramref name="metadata"/> is not the current version.
    /// </exception>
    byte[] Serialize<TMetadata>(TMetadata metadata) where TMetadata : MetadataBase;

    /// <summary>
    /// Deserializes metadata from an export package.
    /// </summary>
    /// <typeparam name="TMetadata">The expected metadata type.</typeparam>
    /// <param name="zipStream">The stream positioned at the start of the metadata entry.</param>
    /// <returns>The deserialized metadata.</returns>
    TMetadata Deserialize<TMetadata>(Stream zipStream) where TMetadata : MetadataBase;
}
