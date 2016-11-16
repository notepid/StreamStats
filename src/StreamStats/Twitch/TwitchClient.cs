using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using StreamStats.Models;

namespace StreamStats.Twitch
{
    public class TwitchClient
    {
        private readonly HttpClient _client;

        public TwitchClient(string clientId)
        {
            _client = new HttpClient { BaseAddress = new Uri("https://api.twitch.tv/kraken/") };
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.twitchtv.v3+json"));
            _client.DefaultRequestHeaders.Add("Client-ID", clientId);
        }

        public TwitchStream GetStreamDetails(string streamName)
        {
            if (string.IsNullOrEmpty(streamName)) throw new ArgumentNullException(nameof(streamName));

            var response = _client.GetAsync($"streams/{streamName}").Result;
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                return null;
            }

            var body = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<TwitchStream>(body);
            return result;
        }
    }
}
