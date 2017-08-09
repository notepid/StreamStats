using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamStats.Models
{
    public class StreamInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool Online { get; set; } = false;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public DateTime StreamStart { get; set; } = DateTime.UtcNow;
        public int MaxViewers { get; set; } = 0;
        public List<string> GamesPlayed { get; set; } = new List<string>();
        public List<int> Viewers { get; set; } = new List<int>();
        public long FollowersStart { get; set; } = 0;
        public long Followers { get; set; } = 0;
        public int OfflineChecksCount { get; set; } = 0;

        public int CalculateAverageViewers()
        {
            if (Viewers.Any())
                return Viewers.Sum() / Viewers.Count;
            return 0;
        }
    }
}
