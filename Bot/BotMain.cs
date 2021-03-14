using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DiscordBot.Bot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordBot.Bot
{

    /// <summary>
    /// Discord authentication options
    /// </summary>
    public class DiscordAuthOptions
    {
        /// <summary>
        /// Discord API authentication token
        /// </summary>
        [Required]
        [NotNull]
        public string DiscordToken { get; init; }
    }
    
    /// <summary>
    /// Configuration options for the bot
    /// </summary>
    public class BotOptions
    {
        /// <summary>
        /// List of prefixes for discord commands
        /// </summary>
        [MinLength(1)]
        [NotNull]
        public List<string> CommandPrefixes { get; init; }
        
        /// <summary>
        /// List of URLs to "anything" images
        /// </summary>
        [NotNull]
        public List<string> SendAnythingUrls { get; init; }
        
        /// <summary>
        /// List of URLs to "motivation" images
        /// </summary>
        [NotNull]
        public List<string> SendMotivationUrls { get; init; }
        
        /// <summary>
        /// List of URLs to "love" images
        /// </summary>
        [NotNull]
        public List<string> SendLoveUrls { get; init; }
        
        /// <summary>
        /// List of URLs to "valication" images
        /// </summary>
        [NotNull]
        public List<string> SendValidationUrls { get; init; }
    }

    public class BotMain
    {
        private readonly DiscordClient        _discord;
        private readonly ILogger<BotMain>     _logger;

        public BotMain(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IOptions<BotOptions> botOptions, IOptions<DiscordAuthOptions> authOptions ,ILogger<BotMain> logger)
        {
            _logger = logger;

            // Create discord client
            _discord = new DiscordClient(
                new DiscordConfiguration
                {
                    Token = authOptions.Value.DiscordToken,
                    TokenType = TokenType.Bot,
                    LoggerFactory = loggerFactory,
                    Intents = DiscordIntents.Guilds | DiscordIntents.DirectMessages | DiscordIntents.GuildMessages,
                }
            );

            // Register commands
            _discord.UseCommandsNext(
                    new CommandsNextConfiguration
                    {
                        StringPrefixes = botOptions.Value.CommandPrefixes,
                        Services = serviceProvider
                    }
                )
                .RegisterCommands<SendImageCommandModule>();
            
            // Log when bot joins a server.
            _discord.GuildCreated += (_, e) =>
            {
                _logger.LogInformation($"Joined server {e.Guild.Id} ({e.Guild.Name})");
                return Task.CompletedTask;
            };
        }
        
        public async Task StartAsync()
        {
            _logger.LogInformation($"Motivation bot {GetType().Assembly.GetName().Version} starting");
            await _discord.ConnectAsync();
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("Motivation bot stopping");
            await _discord.DisconnectAsync();
        }
    }
}