using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheHuntAssistant.Data.File;

public class DataFile
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string BroadcasterId { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public bool Authorized { get; set; }
    public string ClientSecret { get; set; } = "n613ttdcn2y1njdu5aux827r9q9m3q";
}
