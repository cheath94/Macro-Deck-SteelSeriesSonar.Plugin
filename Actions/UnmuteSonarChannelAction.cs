using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.Actions;

public class UnmuteSonarChannelAction : PluginAction
{
    private const string ActionName = "Unmute Sonar Channel";

    public override string Name =>
        ActionName;

    public override string Description =>
        "Unmutes a SteelSeries Sonar channel";

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
                "Unmuting Sonar channel: {0}",
                SonarChannelHelper.GetDisplayName(channel));

            bool succeeded =
                SteelSeriesSonarPlugin.Sonar?
                    .SetMute(
                        channel,
                        false)
                == true;

            if (succeeded)
            {
                SteelSeriesSonarPlugin.RefreshVariables();
            }
            else
            {
                MacroDeckLogger.Warning(
                    SteelSeriesSonarPlugin.Instance!,
                    "Failed to unmute Sonar channel: {0}",
                    SonarChannelHelper.GetDisplayName(channel));
            }
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                SteelSeriesSonarPlugin.Instance!,
                "Unmute Sonar Channel failed: {0}",
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
