﻿using Microsoft.EntityFrameworkCore;
using DifBot.Common.Models;
using System.Threading.Tasks;

namespace DifBot.Data.Repositories
{
    public class BotSettingsRepository
    {
        private readonly BotDbContext _dbContext;

        public BotSettingsRepository(BotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string?> GetBotSetting(string key)
        {
            var setting = await _dbContext.GuildSettings
                .SingleOrDefaultAsync(x => x.Key == key);

            return setting?.Value;
        }

        public async Task<BotSettting> SaveBotSetting(string key, string value)
        {
            var setting = await _dbContext.GuildSettings
                .SingleOrDefaultAsync(x => x.Key == key);

            if (setting == null)
            {
                setting = new BotSettting()
                {
                    Key = key,
                    Value = value
                };

                await _dbContext.AddAsync(setting);
            }
            else
            {
                setting.Value = value;
            }

            await _dbContext.SaveChangesAsync();

            return setting;
        }
    }
}
