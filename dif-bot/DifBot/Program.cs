using System;
using System.Threading.Channels;
using DifBot.CommandHandlers;
using DifBot.Config;
using DifBot.Data;
using DifBot.Data.Repositories;
using DifBot.EventHandlers;
using DifBot.Helpers;
using DifBot.Infrastructure;
using DifBot.Services;
using DifBot.Services.Loggers;
using DifBot.Workers;
using DSharpPlus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DifBot;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<BotOptions>(hostContext.Configuration.GetSection(BotOptions.OptionsKey));
                services.Configure<ForumOptions>(hostContext.Configuration.GetSection(ForumOptions.OptionsKey));

                services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssemblyContaining<Program>();
                    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CommandRequestPipelineBehaviour<,>));
                });

                services.AddTransient<NotificationPublisher>();

                services.AddSingleton<SharedCache>();

                AddWorkers(services);

                AddChannels(services);

                AddBotEventHandlers(services);

                AddInternalServices(services);

                AddRepositories(services);

                AddDbContext(hostContext, services);

                AddDiscordClient(hostContext, services);
            })
            .UseSystemd();
    }

    private static void AddDiscordClient(HostBuilderContext hostContext, IServiceCollection services)
    {
        services.AddSingleton((_) =>
        {
            var defaultLogLevel = hostContext.Configuration.GetValue<string>("Logging:LogLevel:Default") ?? "Warning";
            var botToken = hostContext.Configuration.GetValue<string>("DifBot:BotToken")
                            ?? throw new Exception("Missing BotToken");

            LogLevel logLevel = Enum.Parse<LogLevel>(defaultLogLevel);

            var socketConfig = new DiscordConfiguration
            {
                MinimumLogLevel = logLevel,
                TokenType = TokenType.Bot,
                Token = botToken,
                Intents = DiscordIntents.All,
            };

            var client = new DiscordClient(socketConfig);

            return client;
        });
    }

    private static void AddDbContext(HostBuilderContext hostContext, IServiceCollection services)
    {
        Action<DbContextOptionsBuilder> build = builder =>
        {
            var dbConnString = hostContext.Configuration.GetValue<string>("DifBot:ConnectionString");
            var logLevel = hostContext.Configuration.GetValue<string>("Logging:LogLevel:Default");

            builder.EnableSensitiveDataLogging(logLevel == "Debug");

            builder.UseNpgsql(dbConnString);
        };

        services.AddDbContext<BotDbContext>(build, ServiceLifetime.Transient, ServiceLifetime.Singleton);

        services.AddDbContextFactory<BotDbContext>(build, ServiceLifetime.Singleton);
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddTransient<BotSettingsRepository>();
        services.AddTransient<MessageRefRepository>();
        services.AddTransient<ForumRepository>();

        services.AddSingleton<ForumRepositoryFactory>();
    }

    private static void AddInternalServices(IServiceCollection services)
    {
        services.AddTransient<IDiscordErrorLogger, DiscordErrorLogger>();
        services.AddTransient<DiscordLogger>();
        services.AddTransient<DiscordResolver>();
        services.AddTransient<BotSettingsService>();
        services.AddTransient<MessageRefsService>();
    }

    private static void AddBotEventHandlers(IServiceCollection services)
    {
        services.AddSingleton<CommandEventHandler>();
    }

    private static void AddChannels(IServiceCollection services)
    {
        const int channelSize = 1024;

        services.AddSingleton((_) => new RequestQueueChannel(Channel.CreateBounded<IRequest>(channelSize)));
        services.AddSingleton((_) => new CommandQueueChannel(Channel.CreateBounded<ICommand>(channelSize)));
        services.AddSingleton((_) => new CommandParallelQueueChannel(Channel.CreateBounded<ICommand>(channelSize)));
        services.AddSingleton((_) => new EventQueueChannel(Channel.CreateBounded<INotification>(channelSize)));
        services.AddSingleton((_) => new DiscordLogChannel(Channel.CreateBounded<BaseDiscordLogItem>(channelSize)));
    }

    private static void AddWorkers(IServiceCollection services)
    {
        services.AddHostedService<BotWorker>();
        services.AddHostedService<RequestQueueWorker>();
        services.AddHostedService<CommandQueueWorker>();
        services.AddHostedService<CommandParallelQueueWorker>();
        services.AddHostedService<EventQueueWorker>();
        services.AddHostedService<DiscordLoggerWorker>();
    }
}
