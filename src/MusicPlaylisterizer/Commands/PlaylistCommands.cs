using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlaylisterizer.Commands
{
    public class PlaylistCommands : ModuleBase<SocketCommandContext>
    {
        private readonly YoutubeAppService _service;

        public PlaylistCommands(YoutubeAppService service)
        {
            _service = service;
        }

        [Command("newplaylist")]
        [RequireOwner()]
        [RequireContext(ContextType.Guild)]
        public async Task CreateListAsync()
        {
            ReplyAsync("Hang on, scanning last 200 messages in this channel...");
            var messages = Context.Channel.GetMessagesAsync(200).GetAsyncEnumerator();
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
            links.Reverse();
            ReplyAsync("Okay, creating a youtube playlist...");
            var playlist = await _service.CreatePlaylist(links, $"{Context.User.Username} from {Context.Guild.Name}");
            await ReplyAsync($"Okay, here's your playlist!\n{playlist}");
        }
    }
}
