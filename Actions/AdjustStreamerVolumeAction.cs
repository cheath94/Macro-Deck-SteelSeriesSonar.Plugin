using SteelSeriesSonar.Plugin.Models;
using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.Actions;

public class AdjustStreamerVolumeAction : PluginAction
{
    private const string ActionName =
        "Adjust Streamer Volume";

    public override string Name =>
        ActionName;

    public override string Description =>
        "Raises or lowers a SteelSeries Sonar Streamer volume";

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
                "adjustment",
                -1.0,
                1.0,
                out SonarChannel channel,
                out StreamerOutput output,
                out double adjustment))
        {
            return;
        }

        try
        {
            MacroDeckLogger.Debug(
                SteelSeriesSonarPlugin.Instance!,
                "Adjusting {0} {1} volume by {2:P0}",
                output,
                SonarChannelHelper.GetDisplayName(channel),
                adjustment);

            bool succeeded =
                SteelSeriesSonarPlugin.Sonar?
                    .AdjustStreamerVolume(
                        channel,
                        output,
                        adjustment)
                == true;

            if (!succeeded)
            {
                MacroDeckLogger.Warning(
                    SteelSeriesSonarPlugin.Instance!,
                    "Failed to adjust {0} {1} volume",
                    output,
                    SonarChannelHelper.GetDisplayName(channel));
            }
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                SteelSeriesSonarPlugin.Instance!,
                "Adjust Streamer Volume failed: {0}",
                ex);
        }
    }

    public override ActionConfigControl GetActionConfigControl(
        ActionConfigurator actionConfigurator)
    {
        _ = actionConfigurator;

        return new StreamerVolumeConfigControl(
            this,
            "adjustment",
            "Adjustment (%):",
            minimum: -100,
            maximum: 100,
            defaultValue: 5);
    }
}