using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheHuntAssistant.Data.File;

public class Item
{
    public string Name { get; set; }
    public int Rarity { get; set; }
    public ItemType Type { get; set; }
    public int Points { get; set; }
    public string Map { get; set; }
    public int DLC { get; set; }
    public string DedicatedDropSource { get; set; }
    public bool WorldDrop { get; set; }
    public int MayhemRequirement { get; set; }
    public string ExtraInfo { get; set; }
    public bool Collected { get; set; }
    public string ClipId { get; set; }

    public Item Clone()
    {
        return new Item
        {
            Name = this.Name,
            Rarity = this.Rarity,
            Type = this.Type,
            Points = this.Points,
            Map = this.Map,
            DLC = this.DLC,
            DedicatedDropSource = this.DedicatedDropSource,
            WorldDrop = this.WorldDrop,
            MayhemRequirement = this.MayhemRequirement,
            ExtraInfo = this.ExtraInfo,
            Collected = this.Collected,
            ClipId = this.ClipId
        };
    }
}

public enum ItemType
{
    Pistol,
    Shotgun,
    SMG,
    AR,
    Launcher,
    Sniper,
    Grenade,
    Shield,
    CM,
    Artifact
}