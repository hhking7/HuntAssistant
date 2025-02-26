namespace TheHuntAssistant.Data.Twitch;

public class SubResponse
{
    public List<Data> data { get; set; }
    public int total { get; set; }
    public Pagination pagination { get; set; }

    public class Pagination
    {
        public string cursor { get; set; }
    }

    public class Data
    {
        public string user_name { get; set; }
    }
}