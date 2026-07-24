using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.Actions;

public class SetSonarVolumeAction : PluginAction
{
    private const string ActionName = "Set Sonar Volume";

    public override string Name =>
        ActionName;

    public override string Description =>
        "Changes SteelSeries Sonar volume";

    public override bool CanConfigure =>
        true;

    public override void Trigger(
        string clientId,
        ActionButton actionButton)
    {
        _ = clientId;
        _ = actionButton;

        if (!SonarActionConfiguration.TryReadChannelAndDouble(
                Configuration,
                ActionName,
                "volume",
                0.0,
                1.0,
                out SonarChannel channel,
                out double volume))
        {
            return;
        }

        try
        {
            MacroDeckLogger.Debug(
                SteelSeriesSonarPlugin.Instance!,
                "Setting {0} volume to {1:P0}",
                SonarChannelHelper.GetDisplayName(channel),
                volume);

            bool succeeded =
                SteelSeriesSonarPlugin.Sonar?
                    .SetVolume(
                        channel,
                        volume)
                == true;

            if (succeeded)
            {
                SteelSeriesSonarPlugin.RefreshVariables();
            }
            else
            {
                MacroDeckLogger.Warning(
                    SteelSeriesSonarPlugin.Instance!,
                    "Failed to set {0} volume",
                    SonarChannelHelper.GetDisplayName(channel));
            }
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                SteelSeriesSonarPlugin.Instance!,
                "Set Sonar Volume failed: {0}",
                ex);
        }
    }

    public override ActionConfigControl GetActionConfigControl(
        ActionConfigurator actionConfigurator)
    {
        return new SetSonarVolumeConfigControl(
            this,
            actionConfigurator);
    }
}
