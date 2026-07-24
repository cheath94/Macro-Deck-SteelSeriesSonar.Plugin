using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.Actions;

public class MuteSonarChannelAction : PluginAction
{
    private const string ActionName = "Mute Sonar Channel";

    public override string Name =>
        ActionName;

    public override string Description =>
        "Mutes a SteelSeries Sonar channel";

    public override bool CanConfigure =>
        true;

    public override void Trigger(
        string clientId,
        ActionButton actionButton)
    {
        _ = clientId;
        _ = actionButton;

        if (!SonarActionConfiguration.TryReadChannel(
                Configuration,
                ActionName,
                out SonarChannel channel))
        {
            return;
        }

        try
        {
            MacroDeckLogger.Debug(
                SteelSeriesSonarPlugin.Instance!,
                "Muting Sonar channel: {0}",
                SonarChannelHelper.GetDisplayName(channel));

            bool succeeded =
                SteelSeriesSonarPlugin.Sonar?
                    .SetMute(
                        channel,
                        true)
                == true;

            if (succeeded)
            {
                SteelSeriesSonarPlugin.RefreshVariables();
            }
            else
            {
                MacroDeckLogger.Warning(
                    SteelSeriesSonarPlugin.Instance!,
                    "Failed to mute Sonar channel: {0}",
                    SonarChannelHelper.GetDisplayName(channel));
            }
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                SteelSeriesSonarPlugin.Instance!,
                "Mute Sonar Channel failed: {0}",
                ex);
        }
    }

    public override ActionConfigControl GetActionConfigControl(
        ActionConfigurator actionConfigurator)
    {
        _ = actionConfigurator;

        return new SonarChannelConfigControl(
            this);
    }
}
