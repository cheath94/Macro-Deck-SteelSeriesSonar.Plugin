namespace SteelSeriesSonar.Plugin.Models;

public class StreamerVolumeOutputs
{
    public ClassicVolume Streaming { get; set; } =
        new();

    public ClassicVolume Monitoring { get; set; } =
        new();
}