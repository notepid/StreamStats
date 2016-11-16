using System;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using StreamStats.Discord;
using StreamStats.Logging;
using StreamStats.Models;
using StreamStats.Twitch;

namespace StreamStats
{
    public class Program
    {
        private static StreamChecker _streamChecker;
        private static Timer _timer;
        private static FileLogger _logger;

        public static void Main(string[] args)
        {
            _logger = new FileLogger("streamstats.log", new ConsoleLogger()); ;

            try
            {
                var discordmodel = JsonConvert.DeserializeObject<DiscordModel>(File.ReadAllText(@"discord.json"));
                var twitchModel = JsonConvert.DeserializeObject<TwitchClientModel>(File.ReadAllText(@"twitch.json"));

                var discordClient = new DiscordClient(discordmodel.WebhookUrl, _logger);
                var twitchClient = new TwitchClient(twitchModel.ClientId, _logger);

                if (!Directory.Exists("data"))
                {
                    Directory.CreateDirectory("data");
                }

                _streamChecker = new StreamChecker(twitchClient, discordClient, _logger);

                if (args.Any())
                {
                    if (args.First().ToLower().Equals("-single"))
                    {
                        CheckStreams(_streamChecker);
                        return;
                    }
                }
                else
                {
                    _logger.Log("Starting monitoring streams for stats:");

                    foreach (var line in File.ReadAllLines("twitchusers.txt"))
                    {
                        _logger.Log($"\t{line}");
                    }
                    _logger.Log("Press Q to quit.");
                    CheckStreams(_streamChecker);

                    _timer = new Timer(TimerCallback, null, 60000, Timeout.Infinite);
                    while (Console.ReadKey().Key != ConsoleKey.Q) { }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
            }
        }

        private static void TimerCallback(object state)
        {
            Console.WriteLine(); //Deliberately not using _logger here
            CheckStreams(_streamChecker);
            _timer.Change(60000, Timeout.Infinite);
        }

        public static void CheckStreams(StreamChecker checker)
        {
            foreach (var line in File.ReadAllLines("twitchusers.txt"))
            {
                _logger.Log($"{DateTime.Now} Checking {line}");
                var filename = Path.Combine("data", $"{line}_info.json");

                var streamInfo = new StreamInfo();

                if (File.Exists(filename))
                    streamInfo = JsonConvert.DeserializeObject<StreamInfo>(File.ReadAllText(filename));
                streamInfo.Name = line;

                streamInfo = checker.CheckStream(streamInfo);

                streamInfo.LastUpdated = DateTime.UtcNow;
                File.WriteAllText(filename, JsonConvert.SerializeObject(streamInfo));
            }
        }
    }
}
