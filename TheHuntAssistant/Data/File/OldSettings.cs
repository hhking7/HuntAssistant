using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheHuntAssistant.Data.File;

public class OldSettings1
{
    public string Key { get; set; }
    public string Font { get; set; } = "Compacta Bold Plain";
    public int FontSize { get; set; } = 128;
    public int BorderThickness { get; set; } = 8;
    public string TextColor { get; set; } = "#d3d300";
    public string ListSymbol { get; set; } = "&#9654";
    public int RefreshRate { get; set; } = 2;
    public int TargetWidth { get; set; } = 800;
    public int TargetHeight { get; set; } = 1080;
    public string FarmingText { get; set; } = "Current farm:";
}