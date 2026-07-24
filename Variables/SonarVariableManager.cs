using SteelSeriesSonar.Plugin.Models;
using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;
using SuchByte.MacroDeck.Variables;

namespace SteelSeriesSonar.Plugin.Variables;

public class SonarVariableManager
{
    private readonly MacroDeckPlugin plugin;

    private readonly Dictionary<string, object> lastValues =
        new();

    public event Action<string, object>? VariableChanged;

    public SonarVariableManager(
        MacroDeckPlugin plugin)
    {
        this.plugin = plugin;
    }

    public void UpdateVariables(
        IReadOnlyDictionary<SonarChannel, SonarChannelState> states)
    {
        if (states.Count == 0)
            return;

        bool anyMuted =
            false;

        foreach (KeyValuePair<SonarChannel, SonarChannelState> channel
                 in states)
        {
            string prefix =
                SonarChannelHelper.GetVariablePrefix(
                    channel.Key);

            UpdateChannelVariables(
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

    private void UpdateChannelVariables(
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

    private void UpdateVolumeVariable(
        string name,
        double value)
    {
        float roundedValue =
            (float)Math.Round(
                value,
                3);

        SetVariableIfChanged(
            name,
            roundedValue,
            VariableType.Float);

        UpdateVolumeDisplayVariables(
            name,
            value);
    }

    private void UpdateVolumeDisplayVariables(
        string baseName,
        double value)
    {
        int percent =
            (int)Math.Round(
                value * 100,
                MidpointRounding.AwayFromZero);

        SetVariableIfChanged(
            $"{baseName}_percent",
            percent.ToString(),
            VariableType.String);

        SetVariableIfChanged(
            $"{baseName}_text",
            $"{percent}%",
            VariableType.String);
    }

    private void UpdateMuteVariable(
        string name,
        bool value)
    {
        SetVariableIfChanged(
            name,
            value,
            VariableType.Bool);
    }

    private void SetVariableIfChanged(
        string name,
        object value,
        VariableType type)
    {
        if (lastValues.TryGetValue(
                name,
                out object? previous) &&
            Equals(previous, value))
        {
            return;
        }

        lastValues[name] =
            value;

        VariableManager.SetValue(
            name,
            value,
            type,
            plugin,
            Array.Empty<string>());

        MacroDeckLogger.Debug(
            plugin,
            "Updated variable {0} = {1}",
            name,
            value);

        VariableChanged?.Invoke(
            name,
            value);
    }
}