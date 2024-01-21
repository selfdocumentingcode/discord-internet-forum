using System;
using System.Linq;
using System.Threading.Tasks;
using DifBot.Services.Loggers;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DifBot.Helpers
{
    public class DiscordResolver
    {
        private readonly IDiscordErrorLogger _discordErrorLogger;
        private readonly DiscordClient _client;

        public DiscordResolver(IDiscordErrorLogger discordErrorLogger, DiscordClient client)
        {
            _discordErrorLogger = discordErrorLogger;
            _client = client;
        }

        public DiscordGuild ResolveGuild()
        {
            return _client.Guilds.Select(x => x.Value).First();
        }

        public TryResolveResult<DiscordRole> TryResolveRoleByName(DiscordGuild guild, string discordRoleName)
        {
            var matchingDiscordRoles = guild.Roles
                             .Where(kv => kv.Value.Name.Contains(discordRoleName, StringComparison.OrdinalIgnoreCase))
                             .ToList();

            if (matchingDiscordRoles.Count == 0)
            {
                return TryResolveResult<DiscordRole>.FromError($"No role matches the name {discordRoleName}");
            }
            else if (matchingDiscordRoles.Count > 1)
            {
                return TryResolveResult<DiscordRole>.FromError($"More than 1 role matches the name {discordRoleName}");
            }

            var discordRole = matchingDiscordRoles[0].Value;

            return TryResolveResult<DiscordRole>.FromValue(discordRole);
        }

        public async Task<DiscordMember?> ResolveGuildMember(DiscordGuild guild, ulong userId)
        {
            var memberExists = guild.Members.TryGetValue(userId, out DiscordMember? member);

            if (memberExists) return member;

            try
            {
                return await guild.GetMemberAsync(userId) ?? throw new ArgumentException($"Missing member with id {userId}");
            }
            catch (Exception ex)
            {
                _discordErrorLogger.LogError(ex, "Missing member");

                return null;
            }
        }
    }
}
