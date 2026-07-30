using System.Text.Json.Serialization;

namespace SteelSeriesSonar.Plugin.Models;

public class StreamRedirection
{
    [JsonPropertyName("streamRedirectionId")]
    public string Id { get; set; } =
        string.Empty;

    public string DeviceId { get; set; } =
        string.Empty;

    [JsonPropertyName("status")]
    public List<StreamRedirectionStatus> Statuses { get; set; } =
        new();

    public bool IsRunning { get; set; }
}