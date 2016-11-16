using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using StreamStats.Logging;

namespace StreamStats.Discord
{
    public class DiscordClient
    {
        private readonly ILogger _logger;
        readonly HttpClient _client;

        public DiscordClient(string webhookUrl, ILogger logger)
        {
            _logger = logger;
            _client = new HttpClient { BaseAddress = new Uri(webhookUrl) };
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void SendTextMessage(string message)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("content", message)
            });

            var response = _client.PostAsync("", formContent).Result;
            response.EnsureSuccessStatusCode();
        }
    }
}
