using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamStats.Models
{
    public class StreamInfo
    {
        public bool Online { get; set; } = false;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public int MaxViewers { get; set; } = 0;
        public List<string> GamesPlayed { get; set; } = new List<string>();
        public List<int> Viewers { get; set; } = new List<int>();

        public int CalculateAverageViewers()
        {
            return Viewers.Sum() / Viewers.Count;
        }
    }
}
