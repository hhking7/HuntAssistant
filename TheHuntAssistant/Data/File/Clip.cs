using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheHuntAssistant.Data.File;

public class Clip
{
    public string Id { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string Embed_url { get; set; }
    public string Broadcaster_id { get; set; }
    public string Broadcaster_name { get; set; }
    public string Creator_id { get; set; }
    public string Creator_name { get; set; }
    public string Video_id { get; set; }
    public string Game_id { get; set; }
    public string Title { get; set; }
    public string Created_at { get; set; }
    public float Duration { get; set; }
    public int Vod_offset { get; set; }
    public string Edit_url { get; set; }
    public DateTime Edit_deadline { get; set; }
}