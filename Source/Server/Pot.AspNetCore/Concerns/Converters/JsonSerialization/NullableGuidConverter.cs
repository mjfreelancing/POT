using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pot.AspNetCore.Concerns.Converters.JsonSerialization;

/// <summary>Converts JSON values to and from nullable GUID values.</summary>
public sealed class NullableGuidConverter : JsonConverter<Guid?>
{
    /// <inheritdoc />
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TryGetGuid(out var value) ? value : null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
