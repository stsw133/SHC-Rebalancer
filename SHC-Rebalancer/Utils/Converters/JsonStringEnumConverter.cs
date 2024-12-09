using System.Text.Json.Serialization;
using System.Text.Json;

namespace SHC_Rebalancer;
public class JsonStringEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (Enum.TryParse(value, ignoreCase: true, out T result))
                return result;

            throw new JsonException($"Cannot convert \"{value}\" to enum {typeof(T)}.");
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing enum.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
