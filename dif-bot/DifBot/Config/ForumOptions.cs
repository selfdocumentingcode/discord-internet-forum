using System;

namespace DifBot.Config
{
    public class ForumOptions
    {
        public const string OptionsKey = "ForumOptions";

        public ulong[] RelevantRoleIds { get; init; } = Array.Empty<ulong>();
    }
}
