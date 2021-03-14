using DSharpPlus.Entities;

namespace DiscordBot.Bot.Util
{
    public static class DiscordMessageNewReplyExtension
    {
        /// <summary>
        /// Creates a new DiscordMessageBuilder that is pre-initialized with a reply to the current message.
        /// By default, the reply will not trigger an @mention.
        /// The optional parameter "sendMention" can be set to true to change this behavior.
        /// </summary>
        /// <seealso cref="DiscordMessageBuilder"/>
        /// <seealso cref="DiscordMessage"/>
        /// <param name="message">Message to reply to</param>
        /// <param name="sendMention">If true, the reply will be treated as an @mention. Defaults to false.</param>
        /// <returns>Returns a DiscordMessageBuilder that is pre-configured with the message reply.</returns>
        public static DiscordMessageBuilder NewReply(this DiscordMessage message, bool sendMention = false)
        {
            return new DiscordMessageBuilder().WithReply(message.Id, sendMention);
        }
    }
}