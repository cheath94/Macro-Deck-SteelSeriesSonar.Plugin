using SteelSeriesSonar.Plugin.Models;
using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.Actions;

public class ToggleStreamRedirectionAction : PluginAction
{
    private const string ActionName =
        "Toggle Stream Redirection";

    public override string Name =>
        ActionName;

    public override string Description =>
        "Toggles a SteelSeries Sonar Streamer channel redirection";

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
                "Toggling {0} redirection for {1}",
                SonarChannelHelper.GetDisplayName(channel),
                output);

            bool succeeded =
                SteelSeriesSonarPlugin.Sonar?
                    .ToggleStreamRedirection(
                        channel,
                        output)
                == true;

            if (!succeeded)
            {
                MacroDeckLogger.Warning(
                    SteelSeriesSonarPlugin.Instance!,
                    "Failed to toggle {0} redirection for {1}",
                    SonarChannelHelper.GetDisplayName(channel),
                    output);
            }
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                SteelSeriesSonarPlugin.Instance!,
                "Toggle Stream Redirection failed: {0}",
                ex);
        }
    }

    public override ActionConfigControl GetActionConfigControl(
        ActionConfigurator actionConfigurator)
    {
        _ = actionConfigurator;

        return new StreamerChannelConfigControl(
            this,
            allowMaster: false);
    }
}