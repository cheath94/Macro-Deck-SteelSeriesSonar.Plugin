using SteelSeriesSonar.Plugin.Models;
using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.Actions;

public class SetStreamerVolumeAction : PluginAction
{
    private const string ActionName =
        "Set Streamer Volume";

    public override string Name =>
        ActionName;

    public override string Description =>
        "Sets a SteelSeries Sonar Streamer volume";

    public override bool CanConfigure =>
        true;

    public override void Trigger(
        string clientId,
        ActionButton actionButton)
    {
        _ = clientId;
        _ = actionButton;

        if (!SonarActionConfiguration
            .TryReadStreamerChannelAndDouble(
                Configuration,
                ActionName,
                "volume",
                0.0,
                1.0,
                out SonarChannel channel,
                out StreamerOutput output,
                out double volume))
        {
            return;
        }

        try
        {
            MacroDeckLogger.Debug(
                SteelSeriesSonarPlugin.Instance!,
                "Setting {0} {1} volume to {2:P0}",
                output,
                SonarChannelHelper.GetDisplayName(channel),
                volume);

            bool succeeded =
                SteelSeriesSonarPlugin.Sonar?
                    .SetStreamerVolume(
                        channel,
                        output,
                        volume)
                == true;

            if (!succeeded)
            {
                MacroDeckLogger.Warning(
                    SteelSeriesSonarPlugin.Instance!,
                    "Failed to set {0} {1} volume",
                    output,
                    SonarChannelHelper.GetDisplayName(channel));
            }
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                SteelSeriesSonarPlugin.Instance!,
                "Set Streamer Volume failed: {0}",
                ex);
        }
    }

    public override ActionConfigControl GetActionConfigControl(
        ActionConfigurator actionConfigurator)
    {
        _ = actionConfigurator;

        return new StreamerVolumeConfigControl(
            this,
            "volume",
            "Volume (%):",
            minimum: 0,
            maximum: 100,
            defaultValue: 50);
    }
}