﻿using System;
using System.Linq;
using StreamStats.Discord;
using StreamStats.Models;
using StreamStats.Twitch;

namespace StreamStats
{
    public class StreamChecker
    {
        private readonly TwitchClient _twitchClient;
        private readonly DiscordClient _discordClient;

        public StreamChecker(TwitchClient twitchClient, DiscordClient discordClient)
        {
            _twitchClient = twitchClient;
            _discordClient = discordClient;
        }

        public StreamInfo CheckStream(StreamInfo streamInfo)
        {
            var twitchStream = _twitchClient.GetStreamDetails(streamInfo.Name);
            if (twitchStream == null)
            {
                Console.WriteLine($"Unable to fetch API data for {streamInfo.Name}");
                return streamInfo;
            }

            if (!streamInfo.Online && (twitchStream.stream == null)) return streamInfo; //Was offline, is offline

            if (streamInfo.Online && (twitchStream.stream == null)) //Was online, now offline
            {
                Console.WriteLine("\tStream is now registered as offline");
                streamInfo.Online = false;

                AnnounceStreamOffline(streamInfo);
            }
            else if (!streamInfo.Online && (twitchStream.stream != null)) //Was offline, now online
            {
                Console.WriteLine("\tStream is now registered as online");
                streamInfo = new StreamInfo
                {
                    Online = true,
                    FollowersStart = twitchStream.stream.channel.followers,
                    StreamStart = twitchStream.stream.created_at
                };
                streamInfo.GamesPlayed.Add(twitchStream.stream.game);

                AnnounceStreamOnline(streamInfo, twitchStream);
            }
            else
            {
                if (!streamInfo.GamesPlayed.Contains(twitchStream.stream.game))
                    streamInfo.GamesPlayed.Add(twitchStream.stream.game);

                if (twitchStream.stream.viewers > streamInfo.MaxViewers)
                    streamInfo.MaxViewers = twitchStream.stream.viewers;

                streamInfo.Viewers.Add(twitchStream.stream.viewers);
                streamInfo.Title = twitchStream.stream.channel.status;
                streamInfo.Followers = twitchStream.stream.channel.followers;
            }

            return streamInfo;
        }

        private void AnnounceStreamOnline(StreamInfo streamInfo, TwitchStream twitchStream)
        {
            var message = $"@everyone ***{streamInfo.Name}*** is now live! {twitchStream.stream.channel.url}";
            _discordClient.SendTextMessage(message);
        }

        private void AnnounceStreamOffline(StreamInfo streamInfo)
        {
            var message = $"{streamInfo.Name} is done streaming and here are some stats.\n" +
                                          $"Title: **{streamInfo.Title}**\n";

            if (streamInfo.GamesPlayed.Count > 1)
            {
                message += $"Games: {streamInfo.GamesPlayed.Aggregate(message, (current, game) => current + $"**{game}** ")}\n";
            }
            else if (streamInfo.GamesPlayed.Count == 1)
            {
                message += $"Games: **{streamInfo.GamesPlayed.First()}**\n";
            }

            var duration = DateTime.UtcNow - streamInfo.StreamStart;
            message += $"Duration: **{duration.Hours}h{duration.Minutes}m{duration.Seconds}s**\n" +
                       $"Viewers Average: **{streamInfo.CalculateAverageViewers()}** - High: **{streamInfo.MaxViewers}**\n" +
                       $"Followers Session: **{streamInfo.Followers - streamInfo.FollowersStart}** - Total: **{streamInfo.Followers}**";

            _discordClient.SendTextMessage(message);
        }
    }
}
