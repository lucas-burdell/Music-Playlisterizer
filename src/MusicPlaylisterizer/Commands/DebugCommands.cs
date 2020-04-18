using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicPlaylisterizer.Commands
{
    public class DebugCommands : ModuleBase<SocketCommandContext>
    {

        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync() => ReplyAsync("pong!");

        [Command("list")]
        [RequireOwner()]
        [RequireContext(ContextType.Guild)]
        public async Task ListAsync()
        {
            ReplyAsync("Hang on, getting links in this channel...");
            var messages = Context.Channel.GetMessagesAsync(20).GetAsyncEnumerator();
            var links = new List<string>();
            while (await messages.MoveNextAsync())
            {
                foreach (var message in messages.Current)
                {
                    if (message.Source == MessageSource.User && Uri.IsWellFormedUriString(message.Content, UriKind.Absolute))
                    {
                        links.Add(message.Content);
                    }
                }
            }
            await ReplyAsync($"Links I found: \n{string.Join("\n", links)}");
        }
    }
}
