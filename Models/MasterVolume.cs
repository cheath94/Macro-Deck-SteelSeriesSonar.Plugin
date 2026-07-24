namespace SteelSeriesSonar.Plugin.Models;

/// Represents the master output volume by the Sonar API.

public class MasterVolume
{
    public ClassicVolume Classic { get; set; } = new();

    public StreamVolume Stream { get; set; } = new();
}