using System;
using System.Threading.Tasks;
using DifBot.Data.Repositories;
using DifBot.Infrastructure;

namespace DifBot.Services;

public class BotSettingsService
{
    private static readonly string LastHeartbeatKey = "LastHeartbeat";

    private readonly BotSettingsRepository _botSettingsRepo;

    public BotSettingsService(BotSettingsRepository botSettingsRepos)
    {
        _botSettingsRepo = botSettingsRepos;
    }

    public Task SetLastHeartbeat(DateTimeOffset heartbeatDateTime)
    {
        return _botSettingsRepo.SaveBotSetting(LastHeartbeatKey, heartbeatDateTime.ToUnixTimeMilliseconds().ToString());
    }

    public async Task<DateTimeOffset?> GetLastHeartbeat()
    {
        var lastHeartBeatStringValue = await _botSettingsRepo.GetBotSetting(LastHeartbeatKey);

        if (lastHeartBeatStringValue == null) return null;

        var parsed = long.TryParse(lastHeartBeatStringValue, out var lastHeartBeatTicks);

        if (!parsed) return null;

        return DateTimeOffset.FromUnixTimeMilliseconds(lastHeartBeatTicks);
    }
}
