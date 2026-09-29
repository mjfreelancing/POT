using AllOverIt.Serialization.Binary.Readers;
using Pot.App.Features.Maintenance.Metadata.Models;
using Pot.App.Features.Maintenance.Metadata.Serializer;

namespace Pot.App.Features.Maintenance.Metadata.Readers;

/// <summary>
/// Reads maintenance package metadata written in version 1 of the package format.
/// </summary>
/// <remarks>
/// Version 1 packages are no longer importable; this reader is retained to decode historical metadata for tooling.
/// </remarks>
[Obsolete("Legacy metadata v1 reader retained for historical tracking and possible future preview tooling. Do not use in runtime import/export paths.", error: true)]
internal sealed class MetadataV1Reader : EnrichedBinaryValueReader<MetadataV1>
{
    /// <inheritdoc />
    public override object ReadValue(IEnrichedBinaryReader reader)
    {
        // The version is read by the serializer using this reader.
        var createdAt = MetadataSerializationHelper.ReadCreatedAt(reader);

        return new MetadataV1
        {
            CreatedAt = createdAt
        };
    }
}