using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheHuntAssistant.Data.Twitch;

public class GetClipResponse
{
    public List<Data> data { get; set; }
    public Pagination pagination { get; set; }

    public class Data
    {
        public string id { get; set;}
        public string url { get; set;}
        public string embed_url { get; set;}
        public string broadcaster_id { get; set;}
        public string broadcaster_name { get; set;}
        public string creator_id { get; set;}
        public string creator_name { get; set;}
        public string video_id { get; set;}
        public string game_id { get; set;}
        public string language { get; set;}
        public string title { get; set;}
        public int? view_count { get; set;}
        public string created_at { get; set;}
        public string thumbnail_url { get; set;}
        public float? duration { get; set;}
        public int? vod_offset { get; set;}
    }

    public class Pagination
    {
        public string cursor { get; set; }
    }
}
