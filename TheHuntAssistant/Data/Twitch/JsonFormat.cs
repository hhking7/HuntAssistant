namespace TheHuntAssistant.Data.Twitch;

public class JsonFormat
{
    public string Name { get; set; }
    public string BroadcasterId { get; set; }
    public string Code { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public bool Authorized { get; set; }
    public string ClientSecret { get; set; }

    public JsonFormat()
    {
        ClientSecret = "";
        Authorized = false;
        Name = "";
    }
}