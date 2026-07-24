using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.Actions;

public class ToggleSonarMuteAction : PluginAction
{
    private const string ActionName = "Toggle Sonar Mute";

    public override string Name =>
        ActionName;

    public override string Description =>
        "Toggles mute state for a SteelSeries Sonar channel";

    public override bool CanConfigure =>
        true;

    public override void Trigger(
        string clientId,
        ActionButton actionButton)
    {
        _ = clientId;

        if (!SonarActionConfiguration.TryReadChannel(
                Configuration,
                ActionName,
                out SonarChannel channel))
        {
            return;
        }

        try
        {
            SteelSeriesSonarPlugin.Buttons?
                .RegisterButton(
                    actionButton,
                    SonarChannelHelper.GetMuteVariableName(channel));

            MacroDeckLogger.Debug(
                SteelSeriesSonarPlugin.Instance!,
                "Toggling mute for Sonar channel: {0}",
                SonarChannelHelper.GetDisplayName(channel));

            bool succeeded =
                SteelSeriesSonarPlugin.Sonar?
                    .ToggleMute(channel)
                == true;

            if (succeeded)
            {
                SteelSeriesSonarPlugin.RefreshVariables();
            }
            else
            {
                MacroDeckLogger.Warning(
                    SteelSeriesSonarPlugin.Instance!,
                    "Failed to toggle mute for Sonar channel: {0}",
                    SonarChannelHelper.GetDisplayName(channel));
            }
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                SteelSeriesSonarPlugin.Instance!,
                "Toggle Sonar Mute failed: {0}",
                ex);
        }
    }

    public override ActionConfigControl GetActionConfigControl(
        ActionConfigurator actionConfigurator)
    {
        _ = actionConfigurator;

        return new ToggleSonarMuteConfigControl(
            this);
    }
}
