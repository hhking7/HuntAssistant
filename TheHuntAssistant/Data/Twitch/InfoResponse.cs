namespace TheHuntAssistant.Data.Twitch;

public class InfoResponse
{
    public List<Data> data { get; set; }

    public class Data
    {
        public string id { get; set; }
    }
}