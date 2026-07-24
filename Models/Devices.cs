namespace SteelSeriesSonar.Plugin.Models;

public class Devices
{
    public DeviceVolume Game { get; set; } = new();

    public DeviceVolume ChatRender { get; set; } = new();

    public DeviceVolume ChatCapture { get; set; } = new();

    public DeviceVolume Media { get; set; } = new();

    public DeviceVolume Aux { get; set; } = new();
}