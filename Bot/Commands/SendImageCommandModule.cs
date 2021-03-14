using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Bot.Util;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordBot.Bot.Commands
{
    public class SendImageCommandModule : BaseCommandModule
    {
        private readonly ILogger<SendImageCommandModule> _logger;
        private readonly IOptionsSnapshot<BotOptions> _botOptions;
        private readonly Random _random = new();
        private readonly IDictionary<ulong, int> _lastSentImagesPerChannel = new Dictionary<ulong, int>();

        public SendImageCommandModule(ILogger<SendImageCommandModule> logger, IOptionsSnapshot<BotOptions> botOptions)
        {
            _logger = logger;
            _botOptions = botOptions;
        }

        [Command("anything")]
        [Aliases("general", "something")]
        [RequireBotPermissions(Permissions.SendMessages | Permissions.EmbedLinks)]
        public async Task AnythingCommand(CommandContext ctx)
        {
            // Setup logging context
            using (_logger.BeginScope($"{nameof(AnythingCommand)}@{ctx.Message.Id.ToString()}"))
            {
                _logger.LogDebug("Invoked by [{user}]", ctx.User);
                
                try
                {
                    // Get options snapshot
                    var botOptions = _botOptions.Value;
                    
                    // Combine all categories
                    // TODO cache this somehow?
                    var allImagesSet = MergeCollections(
                        botOptions.SendAnythingUrls,
                        botOptions.SendMotivationUrls,
                        botOptions.SendLoveUrls,
                        botOptions.SendValidationUrls);
                    var allImagesList = allImagesSet.ToList();
                    
                    // Send an image from any category
                    await SendRandomImage(ctx, allImagesList);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Uncaught exception");
                }
            }
        }
        
        [Command("motivation")]
        [Aliases("encouragement")]
        [RequireBotPermissions(Permissions.SendMessages | Permissions.EmbedLinks)]
        public async Task MotivationCommand(CommandContext ctx)
        {
            // Setup logging context
            using (_logger.BeginScope($"{nameof(MotivationCommand)}@{ctx.Message.Id.ToString()}"))
            {
                _logger.LogDebug("Invoked by [{user}]", ctx.User);
                
                try
                {
                    // Send a "motivation" image
                    var imageList = _botOptions.Value.SendMotivationUrls;
                    await SendRandomImage(ctx, imageList);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Uncaught exception");
                }
            }
        }
        
        [Command("love")]
        [Aliases("affection")]
        [RequireBotPermissions(Permissions.SendMessages | Permissions.EmbedLinks)]
        public async Task LoveCommand(CommandContext ctx)
        {
            // Setup logging context
            using (_logger.BeginScope($"{nameof(LoveCommand)}@{ctx.Message.Id.ToString()}"))
            {
                _logger.LogDebug("Invoked by [{user}]", ctx.User);
                
                try
                {
                    // Send a "love" image
                    var imageList = _botOptions.Value.SendLoveUrls;
                    await SendRandomImage(ctx, imageList);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Uncaught exception");
                }
            }
        }
        
        [Command("validation")]
        [RequireBotPermissions(Permissions.SendMessages | Permissions.EmbedLinks)]
        public async Task ValidationCommand(CommandContext ctx)
        {
            // Setup logging context
            using (_logger.BeginScope($"{nameof(ValidationCommand)}@{ctx.Message.Id.ToString()}"))
            {
                _logger.LogDebug("Invoked by [{user}]", ctx.User);
                
                try
                {
                    // Send a "love" image
                    var imageList = _botOptions.Value.SendValidationUrls;
                    await SendRandomImage(ctx, imageList);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Uncaught exception");
                }
            }
        }

        private async Task SendRandomImage(CommandContext ctx, IList<string> imageList)
        {
            // Make sure that there are images for this category
            if (imageList == null || !imageList.Any())
            {
                _logger.LogDebug("Image list is empty");
                await ctx.Message.NewReply().WithContent("Oops! I don't have any images of that type.").SendAsync(ctx.Channel);
                return;
            }
                    
            // Pick a random image
            var imageUrl = PickRandomImage(imageList, ctx.Channel.Id);
            _logger.LogDebug("Sending {url}", imageUrl);
                    
            // Send reply
            await ctx.Message.NewReply()
                .WithEmbed(new DiscordEmbedBuilder()
                    .WithImageUrl(imageUrl)
                    .Build()
                )
                .SendAsync(ctx.Channel);
        }

        private string PickRandomImage(IList<string> imageList, ulong channelId)
        {
            if (imageList == null || !imageList.Any()) throw new IndexOutOfRangeException("Image list cannot be empty");
            
            // Pick a random, non-duplicate image index
            int idx;
            do
            {
                // Pick a random image
                idx = _random.Next(imageList.Count);
            }
            // IF there are other options AND we just sent this one, then pick another
            while (imageList.Count > 1 && _lastSentImagesPerChannel.TryGetValue(channelId, out var lastIdx) && lastIdx == idx);
            
            // Record our choice
            _lastSentImagesPerChannel[channelId] = idx;

            // Get the image URL by index
            return imageList[idx];
        }

        private static HashSet<T> MergeCollections<T>(params ICollection<T>[] collections)
        {
            var set = new HashSet<T>();

            // Add each collection
            foreach (var collection in collections)
            {
                // Skip null collections
                if (collection == null) continue;
                
                // Add all the contents
                foreach (var item in collection)
                {
                    set.Add(item);
                }
            }
            
            return set;
        }
    }
}