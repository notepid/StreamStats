using System;
// ReSharper disable InconsistentNaming

namespace StreamStats.Models
{
    public class TwitchChannel
    {
        public bool mature { get; set; }
        public string status { get; set; }
        public string broadcaster_language { get; set; }
        public string display_name { get; set; }
        public string game { get; set; }
        public string language { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public bool partner { get; set; }
        public long views { get; set; }
        public long followers { get; set; }
    }
}