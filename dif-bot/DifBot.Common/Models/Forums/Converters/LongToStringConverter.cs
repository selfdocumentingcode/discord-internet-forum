using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DifBot.Common.Models.Forums.Converters;

public class LongToStringConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string stringValue = reader.GetString() ?? default(long).ToString();
            if (long.TryParse(stringValue, out long value))
                return value;
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
