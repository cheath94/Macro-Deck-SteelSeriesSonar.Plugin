namespace SteelSeriesSonar.Plugin.Models;

public class StreamerDeviceVolumes
{
    public StreamerVolumeCollection Game { get; set; } =
        new();

    public StreamerVolumeCollection ChatRender { get; set; } =
        new();

    public StreamerVolumeCollection ChatCapture { get; set; } =
        new();

    public StreamerVolumeCollection Media { get; set; } =
        new();

    public StreamerVolumeCollection Aux { get; set; } =
        new();
}