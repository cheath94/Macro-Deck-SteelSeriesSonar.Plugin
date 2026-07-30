namespace SteelSeriesSonar.Plugin.Models;

public class StreamerVolumeSettings
{
    public StreamerVolumeCollection Masters { get; set; } =
        new();

    public StreamerDeviceVolumes Devices { get; set; } =
        new();
}