using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SHC_Rebalancer;
public class SingleLineArrayConverterFactory : JsonConverterFactory
{
    /// CanConvert
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsArray;
    }

    /// CreateConverter
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var elementType = typeToConvert.GetElementType()!;
        var converterType = typeof(SingleLineArrayConverter<>).MakeGenericType(elementType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class SingleLineArrayConverter<T> : JsonConverter<T[]>
{
    /// Read
    public override T[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            reader.Read();
            return null;
        }

        var list = new List<T>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;
            list.Add(JsonSerializer.Deserialize<T>(ref reader, options)!);
        }
        return [.. list];
    }

    /// Write
    public override void Write(Utf8JsonWriter writer, T[]? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        using var ms = new MemoryStream();
        using (var noIndentWriter = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = false }))
        {
            noIndentWriter.WriteStartArray();

            foreach (var item in value)
                JsonSerializer.Serialize(noIndentWriter, item, options);

            noIndentWriter.WriteEndArray();
        }

        var arrayJson = Encoding.UTF8.GetString(ms.ToArray());
        arrayJson = System.Text.RegularExpressions.Regex.Replace(
            arrayJson,
            @",(?!\s)",
            ", "
        );

        writer.WriteRawValue(arrayJson);
    }
}
