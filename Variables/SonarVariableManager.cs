using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;
using SuchByte.MacroDeck.Variables;

namespace SteelSeriesSonar.Plugin.Variables;

public partial class SonarVariableManager
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

    private void UpdateVolumeVariable(
        string name,
        double value)
    {
        double clampedValue =
            Math.Clamp(
                value,
                0.0,
                1.0);

        float roundedValue =
            (float)Math.Round(
                clampedValue,
                3);

        SetVariableIfChanged(
            name,
            roundedValue,
            VariableType.Float);

        UpdateVolumeDisplayVariables(
            name,
            clampedValue);
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
