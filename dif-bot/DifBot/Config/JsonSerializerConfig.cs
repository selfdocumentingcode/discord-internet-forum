using System.Text.Json;
using DifBot.Common.Models.Forums.Converters;

namespace DifBot.Config;

public static class JsonSerializerConfig
{
    public static readonly JsonSerializerOptions PublishOptions = new JsonSerializerOptions
    {
        Converters =
        {
            new PostReactionConverter(),
            new LongToStringConverter(),
            new ULongToStringConverter(),
        },
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
