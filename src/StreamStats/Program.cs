using System;
using System.IO;
using Newtonsoft.Json;
using StreamStats.Discord;
using StreamStats.Models;
using StreamStats.Twitch;

namespace StreamStats
{
    public class Program
    {
        private static TwitchClient _twitchClient;
        private static DiscordClient _discordClient;

        public static void Main(string[] args)
        {
            var discordmodel = JsonConvert.DeserializeObject<DiscordModel>(File.ReadAllText(@"discord.json"));
            var twitchModel = JsonConvert.DeserializeObject<TwitchClientModel>(File.ReadAllText(@"twitch.json"));

            _discordClient = new DiscordClient(discordmodel.WebhookUrl);
            _twitchClient = new TwitchClient(twitchModel.ClientId);

            if (!Directory.Exists("data"))
            {
                Directory.CreateDirectory("data");
            }

            CheckStreams();

            Console.ReadKey();
        }

        public static void CheckStreams()
        {
            var lines = File.ReadAllLines("twitchusers.txt");

            foreach (var line in lines)
            {
                CheckStream(line);
            }
        }

        public static void CheckStream(string streamName)
        {
            Console.WriteLine($"Checking {streamName}");
            var filename = $"data\\{streamName}_info.json";
            var streamInfo = new StreamInfo();

            if (File.Exists(filename))
                streamInfo = JsonConvert.DeserializeObject<StreamInfo>(File.ReadAllText(filename));

            var twitchStream = _twitchClient.GetStreamInfo(streamName);

            if (streamInfo.Online && (twitchStream.stream == null) ) //Was online, now offline
            {
                Console.WriteLine("\tStream is now registered as offline");
                streamInfo.Online = false;
            }
            else if (!streamInfo.Online && (twitchStream.stream != null)) //Was offline, now online
            {
                Console.WriteLine("\tStream is now registered as online");
                streamInfo = new StreamInfo { Online = true };
                streamInfo.GamesPlayed.Add(twitchStream.stream.game);

                var message = $"@everyone {streamName} is now live! {twitchStream.stream.channel.url}";
                _discordClient.SendTextMessage(message);
            }

            if (streamInfo.Online && (twitchStream.stream != null)) //Was online, still online
            {
                if (!streamInfo.GamesPlayed.Contains(twitchStream.stream.game))
                    streamInfo.GamesPlayed.Add(twitchStream.stream.game);

                if (twitchStream.stream.viewers > streamInfo.MaxViewers)
                    streamInfo.MaxViewers = twitchStream.stream.viewers;

                streamInfo.Viewers.Add(twitchStream.stream.viewers);
            }

            streamInfo.LastUpdated = DateTime.UtcNow;
            File.WriteAllText(filename, JsonConvert.SerializeObject(streamInfo));
        }
    }
}
