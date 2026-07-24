namespace SteelSeriesSonar.Plugin.Models;

public class DeviceVolume
{
    public ClassicVolume Classic { get; set; } = new();

    public StreamVolume Stream { get; set; } = new();
}