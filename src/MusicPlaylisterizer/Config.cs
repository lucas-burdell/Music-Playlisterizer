using Microsoft.Extensions.Configuration;

namespace MusicPlaylisterizer
{
    public class Config
    {
        private readonly IConfiguration config;

        public Config(IConfiguration config)
        {
            this.config = config;
        }

        public string DiscordToken { get => config["discordToken"]; }
        public string YoutubeClientId { get => config["youtubeClientId"]; }
        public string YoutubeClientSecret { get => config["youtubeClientSecret"];  }
        public string YoutubeEmail { get => config["youtubeEmail"]; }
        public string YoutubePlaylist { get => config["youtubePlaylist"]; }
        public string YoutubeAsJson { get => config["youtube"]; }
    }
}
