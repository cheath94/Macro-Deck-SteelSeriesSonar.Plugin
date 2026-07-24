namespace SteelSeriesSonar.Plugin.Models;

public class VolumeSettings
{
    public MasterVolume Masters { get; set; } = new();

    public Devices Devices { get; set; } = new();
}