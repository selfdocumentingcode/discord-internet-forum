using System;

namespace DifBot.Helpers;

public static class DateExtensions
{
    public static ulong ToSnowflake(this DateTimeOffset dateTime)
    {
        long discordEpochMs = DiscordConstants.DiscordEpochMs;
        long dateTimeMs = dateTime.ToUnixTimeMilliseconds();

        long snowflake = dateTimeMs - discordEpochMs << 22;

        return (ulong)snowflake;
    }
}
