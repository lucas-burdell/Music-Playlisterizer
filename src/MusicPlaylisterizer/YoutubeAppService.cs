using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MusicPlaylisterizer
{
    public class YoutubeAppService
    {
        private readonly Config _config;
        private YouTubeService _service;
        private bool _isReady = false;

        public YoutubeAppService(Config config) => _config = config;

        public async Task InitializeAsync()
        {
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets()
                {
                    ClientId = _config.YoutubeClientId,
                    ClientSecret = _config.YoutubeClientSecret
                },
                new[]
                {
                        YouTubeService.Scope.Youtube,
                        YouTubeService.Scope.Youtubepartner,
                        YouTubeService.Scope.YoutubeUpload,
                        YouTubeService.Scope.YoutubepartnerChannelAudit,
                        YouTubeService.Scope.YoutubeReadonly,
                        YouTubeService.ScopeConstants.Youtube,
                        YouTubeService.ScopeConstants.YoutubeReadonly,
                        YouTubeService.ScopeConstants.YoutubeUpload
                },
                "user",
                default(CancellationToken),
                new FileDataStore(GetType().ToString()));


            _service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "music-playlisterizer"
            });
            _isReady = true;
        }

        private bool IsYoutubeVideo(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return false;
            }
            var uri = new Uri(url);
            return uri.Host == "youtu.be" || uri.Host.EndsWith("youtube.com");
        }

        private string ExtractYoutubeVideoId(string url)
        {
            var uri = new Uri(url);
            if (uri.Host == "youtu.be")
            {
                return uri.LocalPath.Trim('/');
            }
            if (uri.Host.EndsWith("youtube.com"))
            {
                return HttpUtility.ParseQueryString(uri.Query)["v"];
            }
            throw new ArgumentException($"{url} is not a valid Youtube video.");
        }

        private async Task AddVideoToPlaylist(string playlistId, string videoId)
        {
            var newItem = new PlaylistItem
            {
                Snippet = new PlaylistItemSnippet
                {
                    PlaylistId = playlistId,
                    ResourceId = new ResourceId
                    {
                        Kind = "youtube#video",
                        VideoId = videoId
                    }
                }
            };
            try
            {
                await _service.PlaylistItems.Insert(newItem, "snippet").ExecuteAsync();
            }
            catch (Exception)
            {
            }
        }

        public async Task<string> CreatePlaylist(IEnumerable<string> videos, string creator)
        {
            if (!_isReady)
            {
                return "Youtube service isn't ready!";
            }
            DateTime timestamp = DateTime.UtcNow;
            var newPlaylist = new Playlist
            {
                Snippet = new PlaylistSnippet
                {
                    Title = $"Music {timestamp.ToString()}",
                    Description = $"A playlist created at {timestamp.ToFileTimeUtc()} initiated by {creator}"
                },
                Status = new PlaylistStatus
                {
                    PrivacyStatus = "public"
                }
            };
            newPlaylist = await _service.Playlists.Insert(newPlaylist, "snippet,status").ExecuteAsync();

            string firstVideoId = null;

            foreach (var video in videos)
            {
                if (!IsYoutubeVideo(video))
                {
                    Console.WriteLine($"{video} is not a video");
                    continue;
                }
                var id = ExtractYoutubeVideoId(video);
                var tasks = new List<Task>();
                if (firstVideoId == null)
                {
                    firstVideoId = id;
                }
                tasks.Add(AddVideoToPlaylist(newPlaylist.Id, id));

                Task.WaitAll(tasks.ToArray());
            }

            return $"https://www.youtube.com/watch?v={firstVideoId}&list={newPlaylist.Id}";
        }
    }
}
