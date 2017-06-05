using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace StreamStats.Models
{
    public class TwitchStreamDetails
    {
        public string game { get; set; }
        public string stream_type { get; set; }
        public int viewers { get; set; }
        public int delay { get; set; }
        public int video_height { get; set; }
        public bool is_playlist { get; set; }
        public DateTime created_at { get; set; }
        public long _id { get; set; }
        public TwitchChannel channel { get; set; }
    }
}
