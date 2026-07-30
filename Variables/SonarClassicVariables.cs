using SteelSeriesSonar.Plugin.Models;
using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.Variables;

namespace SteelSeriesSonar.Plugin.Variables;

public partial class SonarVariableManager
{
    public void UpdateVariables(
        IReadOnlyDictionary<SonarChannel, SonarChannelState> states)
    {
        if (states.Count == 0)
        {
            return;
        }

        bool anyMuted =
            false;

        foreach (KeyValuePair<SonarChannel, SonarChannelState> channel
                 in states)
        {
            string prefix =
                SonarChannelHelper.GetVariablePrefix(
                    channel.Key);

            UpdateClassicChannelVariables(
                prefix,
                channel.Value);

            anyMuted |=
                channel.Value.Muted;
        }

        SetVariableIfChanged(
            "sonar_any_muted",
            anyMuted,
            VariableType.Bool);
    }

    private void UpdateClassicChannelVariables(
        string prefix,
        SonarChannelState state)
    {
        UpdateVolumeVariable(
            $"{prefix}_volume",
            state.Volume);

        UpdateMuteVariable(
            $"{prefix}_muted",
            state.Muted);
    }
}
