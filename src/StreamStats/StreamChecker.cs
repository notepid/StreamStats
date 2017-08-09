using System;
using System.Linq;
using StreamStats.Discord;
using StreamStats.Logging;
using StreamStats.Models;
using StreamStats.Twitch;

namespace StreamStats
{
    public class StreamChecker
    {
        private readonly TwitchClient _twitchClient;
        private readonly DiscordClient _announcementDiscordClient;
        private readonly DiscordClient _statsDiscordClient;
        private readonly ILogger _logger;
        private const int NumOfflineChecks = 5;

        public StreamChecker(TwitchClient twitchClient, DiscordClient announcementDiscordClient, DiscordClient statsDiscordClient, ILogger logger)
        {
            _twitchClient = twitchClient;
            _announcementDiscordClient = announcementDiscordClient;
            _statsDiscordClient = statsDiscordClient;
            _logger = logger;
        }

        public StreamInfo CheckStream(StreamInfo streamInfo)
        {
            var twitchStream = _twitchClient.GetStreamDetails(streamInfo.Name);
            if (twitchStream == null)
            {
                _logger.Log(LoggingEventType.Error, $"Unable to fetch API data for {streamInfo.Name}");
                return streamInfo;
            }

            if (!streamInfo.Online && (twitchStream.stream == null)) return streamInfo; //Was offline, is offline

            if (streamInfo.Online && (twitchStream.stream == null)) //Was online, now offline
            {
                if (streamInfo.OfflineChecksCount >= NumOfflineChecks)
                {

                    _logger.Log("\tStream is now registered as offline");
                    streamInfo.Online = false;

                    AnnounceStreamOffline(streamInfo);
                }
                else
                {
                    streamInfo.OfflineChecksCount++;
                    _logger.Log($"\tTwitch API says stream is offline now, but will check again to be sure. {streamInfo.OfflineChecksCount} of {NumOfflineChecks} checks made.");
                }
            }
            else if (twitchStream.stream != null && !twitchStream.stream.stream_type.Equals("live")) //online with VOD or some other stupid shit - Count this as offline
            {
                _logger.Log("\tOnline with VOD - Counts as offline");
                streamInfo.Online = false;
            }
            else if (!streamInfo.Online && twitchStream.stream != null && twitchStream.stream.stream_type.Equals("live") ) //Was offline, now online
            {
                _logger.Log("\tStream is now registered as online");
                var name = streamInfo.Name;
                streamInfo = new StreamInfo
                {
                    Online = true,
                    Name = name,
                    FollowersStart = twitchStream.stream.channel.followers,
                    StreamStart = twitchStream.stream.created_at,
                    OfflineChecksCount = 0
                };
                streamInfo.GamesPlayed.Add(twitchStream.stream.game);

                AnnounceStreamOnline(streamInfo, twitchStream);
            }
            else
            {
                if (!streamInfo.GamesPlayed.Contains(twitchStream.stream.game)) //Streamer changed game
                    streamInfo.GamesPlayed.Add(twitchStream.stream.game);
                
                if (twitchStream.stream.viewers > streamInfo.MaxViewers) //New peak viewer count
                    streamInfo.MaxViewers = twitchStream.stream.viewers;

                streamInfo.Viewers.Add(twitchStream.stream.viewers);
                streamInfo.Title = twitchStream.stream.channel.status;
                streamInfo.Followers = twitchStream.stream.channel.followers;
            }

            return streamInfo;
        }

        private void AnnounceStreamOnline(StreamInfo streamInfo, TwitchStream twitchStream)
        {
            var message = $"@everyone ***{streamInfo.Name}*** is now live!\n" +
                          $"{twitchStream.stream.game}\n" +
                          $"{twitchStream.stream.channel.status}\n" +
                          $"{twitchStream.stream.channel.url}";
            _announcementDiscordClient.SendTextMessage(message);
        }

        private void AnnounceStreamOffline(StreamInfo streamInfo)
        {
            var message = $"{streamInfo.Name} is done streaming and here are some stats.\n" +
                                          $"Title: **{streamInfo.Title}**\n";

            if (streamInfo.GamesPlayed.Count > 1)
            {
                var games = "Games:\n";
                foreach (var game in streamInfo.GamesPlayed)
                {
                    games += $"**{game}**\n";
                }
                message += games;
            }
            else if (streamInfo.GamesPlayed.Count == 1)
            {
                message += $"Games: **{streamInfo.GamesPlayed.First()}**\n";
            }

            var duration = DateTime.UtcNow - streamInfo.StreamStart;
            message += $"Duration: **{duration.Hours}h{duration.Minutes}m{duration.Seconds}s**\n" +
                       $"Viewers Average: **{streamInfo.CalculateAverageViewers()}** - High: **{streamInfo.MaxViewers}**\n" +
                       $"Followers Session: **{streamInfo.Followers - streamInfo.FollowersStart}** - Total: **{streamInfo.Followers}**";

            _statsDiscordClient.SendTextMessage(message);
        }
    }
}
