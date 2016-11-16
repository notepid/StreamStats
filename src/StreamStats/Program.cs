using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using StreamStats.Discord;
using StreamStats.Models;
using StreamStats.Twitch;

namespace StreamStats
{
    public class Program
    {
        private static StreamChecker _streamChecker;
        private static Timer _timer;

        public static void Main(string[] args)
        {
            var discordmodel = JsonConvert.DeserializeObject<DiscordModel>(File.ReadAllText(@"discord.json"));
            var twitchModel = JsonConvert.DeserializeObject<TwitchClientModel>(File.ReadAllText(@"twitch.json"));

            var discordClient = new DiscordClient(discordmodel.WebhookUrl);
            var twitchClient = new TwitchClient(twitchModel.ClientId);

            if (!Directory.Exists("data"))
            {
                Directory.CreateDirectory("data");
            }

            _streamChecker = new StreamChecker(twitchClient, discordClient);

            CheckStreams(_streamChecker);

            if (args != null)
            {
                if (args[0].ToLower().Equals("-single"))
                    return;
            }
            else
            {
                Console.WriteLine("Starting monitoring streams for stats:");

                foreach (var line in File.ReadAllLines("twitchusers.txt"))
                {
                    Console.WriteLine($"\t{line}");
                }

                _timer = new Timer(TimerCallback, null, 60000, Timeout.Infinite);
                while (Console.ReadKey().Key != ConsoleKey.Q) { }
            }
        }

        private static void TimerCallback(object state)
        {
            CheckStreams(_streamChecker);
            _timer.Change(60000, Timeout.Infinite);
        }

        public static void CheckStreams(StreamChecker checker)
        {
            foreach (var line in File.ReadAllLines("twitchusers.txt"))
            {
                Console.WriteLine($"{DateTime.Now} Checking {line}");
                var filename = $"data\\{line}_info.json";

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
