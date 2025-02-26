using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheHuntAssistant.Data.Twitch;

public class CreateClipResponse
{
    public List<Data> data { get; set; }

    public class Data
    {
        public string id { get; set; }
        public string edit_url { get; set; }
    }
}
