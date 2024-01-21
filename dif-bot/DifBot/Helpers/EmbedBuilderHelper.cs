﻿using System;
using DSharpPlus.Entities;

namespace DifBot.Helpers;

public static class EmbedBuilderHelper
{
    public static DiscordEmbed BuildSimpleEmbed(string title, string message)
    {
        return BuildSimpleEmbed(title, message, DiscordConstants.DefaultEmbedColor);
    }

    public static DiscordEmbed BuildSimpleEmbed(string title, string message, int color)
    {
        var truncatedMessage = message.Substring(0, Math.Min(message.Length, DiscordConstants.MaxEmbedContentLength));

        var eb = new DiscordEmbedBuilder()
            .WithTitle(title)
            .WithDescription(truncatedMessage)
            .WithColor(color);

        return eb.Build();
    }
}
