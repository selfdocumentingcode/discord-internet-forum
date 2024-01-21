using System;

namespace DifBot.Common.Extensions;

public static class DateTimeExtensions
{
    public static string ToIsoDateTimeString(this DateTimeOffset dateTimeOffset)
    {
        return $"{dateTimeOffset:yyyy-MM-dd HH:mm}";
    }
}
