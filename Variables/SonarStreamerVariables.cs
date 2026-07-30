using SteelSeriesSonar.Plugin.Models;
using SteelSeriesSonar.Plugin.Sonar;

namespace SteelSeriesSonar.Plugin.Variables;

public partial class SonarVariableManager
{
    public void UpdateStreamerVariables(
        IReadOnlyDictionary<
            (StreamerOutput Output, SonarChannel Channel),
            ClassicVolume> states)
    {
        if (states.Count == 0)
        {
            return;
        }

        foreach (KeyValuePair<
                     (StreamerOutput Output, SonarChannel Channel),
                     ClassicVolume> state
                 in states)
        {
            string prefix =
                GetStreamerVariablePrefix(
                    state.Key.Output,
                    state.Key.Channel);

            UpdateVolumeVariable(
                $"{prefix}_volume",
                state.Value.Volume);

            UpdateMuteVariable(
                $"{prefix}_muted",
                state.Value.Muted);
        }
    }

    private static string GetStreamerVariablePrefix(
        StreamerOutput output,
        SonarChannel channel)
    {
        string outputName =
            output.ToString()
                .ToLowerInvariant();

        string classicPrefix =
            SonarChannelHelper.GetVariablePrefix(
                channel);

        string channelName =
            classicPrefix.StartsWith(
                "sonar_",
                StringComparison.Ordinal)
                    ? classicPrefix["sonar_".Length..]
                    : classicPrefix;

        return
            $"sonar_{outputName}_{channelName}";
    }
}
