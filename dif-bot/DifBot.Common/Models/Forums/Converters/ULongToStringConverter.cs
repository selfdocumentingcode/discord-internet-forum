using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DifBot.Common.Models.Forums.Converters;

public class ULongToStringConverter : JsonConverter<ulong>
{
    public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string stringValue = reader.GetString() ?? default(ulong).ToString();
            if (ulong.TryParse(stringValue, out ulong value))
                return value;
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
