using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MusicPlaylisterizer
{
    internal class Program
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandHandlingService _commands;
        private readonly Config _config;
        private readonly YoutubeAppService _youtube;

        // There is no need to implement IDisposable like before as we are
        // using dependency injection, which handles calling Dispose for us.
        private static void Main(string[] args)
        {
            using (ServiceProvider services = ConfigureServices())
            {
                Program program = services.GetRequiredService<Program>();
                Task.WaitAll(program.MainAsync(), Task.Delay(-1));
            }
        }

        public Program(DiscordSocketClient client, CommandHandlingService commands, Config config, YoutubeAppService youtube)
        {
            _client = client;
            _commands = commands;
            _config = config;
            _youtube = youtube;
        }

        public async Task MainAsync()
        {
            _client.Log += LogAsync;

            await _client.LoginAsync(TokenType.Bot, _config.DiscordToken);
            await _client.StartAsync();
            await _youtube.InitializeAsync();
            await _commands.InitializeAsync();
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private static Config GetConfig()
        {

            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");

            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) ||
                                devEnvironmentVariable.ToLower() == "development";
            if (isDevelopment)
            {
                builder.AddUserSecrets<Program>();
            }


            return new Config(builder.Build());
        }


        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(GetConfig())
                .AddSingleton<YoutubeAppService>()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<Program>()
                .BuildServiceProvider();
        }
    }
}
