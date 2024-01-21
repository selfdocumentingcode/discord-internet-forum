using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DifBot.Common.Models.Forums.Converters;

public class PostReactionConverter : JsonConverter<PostReaction>
{
    public override PostReaction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);

        var root = doc.RootElement;

        if (root.TryGetProperty(nameof(DiscordEmoji.DiscordEmojiId), out var _))
        {
            return JsonSerializer.Deserialize<DiscordEmojiReaction>(root.GetRawText())
                ?? throw new InvalidOperationException("Invalid PostReaction type");
        }
        else if (root.TryGetProperty(nameof(UnicodeEmojiReaction.EmojiCodePoint), out var _))
        {
            return JsonSerializer.Deserialize<UnicodeEmojiReaction>(root.GetRawText())
                ?? throw new InvalidOperationException("Invalid PostReaction type");
        }

        throw new InvalidOperationException("Invalid PostReaction type");
    }

    public override void Write(Utf8JsonWriter writer, PostReaction value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, options);
    }
}
