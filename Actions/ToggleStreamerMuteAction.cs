using SteelSeriesSonar.Plugin.Models;
using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.Actions;

public class ToggleStreamerMuteAction : PluginAction
{
    private const string ActionName =
        "Toggle Streamer Mute";

    public override string Name =>
        ActionName;

    public override string Description =>
        "Toggles mute for a SteelSeries Sonar Streamer channel";

    public override bool CanConfigure =>
        true;

    public override void Trigger(
        string clientId,
        ActionButton actionButton)
    {
        _ = clientId;
        _ = actionButton;

        if (!SonarActionConfiguration
            .TryReadStreamerChannel(
                Configuration,
                ActionName,
                out SonarChannel channel,
                out StreamerOutput output))
        {
            return;
        }

        try
        {
            MacroDeckLogger.Debug(
                SteelSeriesSonarPlugin.Instance!,
                "Toggling {0} {1} mute",
                output,
                SonarChannelHelper.GetDisplayName(channel));

            bool succeeded =
                SteelSeriesSonarPlugin.Sonar?
                    .ToggleStreamerMute(
                        channel,
                        output)
                == true;

            if (!succeeded)
            {
                MacroDeckLogger.Warning(
                    SteelSeriesSonarPlugin.Instance!,
                    "Failed to toggle {0} {1} mute",
                    output,
                    SonarChannelHelper.GetDisplayName(channel));
            }
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                SteelSeriesSonarPlugin.Instance!,
                "Toggle Streamer Mute failed: {0}",
                ex);
        }
    }

    public override ActionConfigControl GetActionConfigControl(
        ActionConfigurator actionConfigurator)
    {
        _ = actionConfigurator;

        return new StreamerChannelConfigControl(
            this);
    }
}