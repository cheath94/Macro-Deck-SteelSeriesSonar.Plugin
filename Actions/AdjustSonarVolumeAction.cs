using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.Actions;

public class AdjustSonarVolumeAction : PluginAction
{
    private const string ActionName = "Adjust Sonar Volume";

    public override string Name =>
        ActionName;

    public override string Description =>
        "Raises or lowers a SteelSeries Sonar volume channel.";

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
                "adjustment",
                -1.0,
                1.0,
                out SonarChannel channel,
                out double adjustment))
        {
            return;
        }

        try
        {
            MacroDeckLogger.Debug(
                SteelSeriesSonarPlugin.Instance!,
                "Adjusting {0} volume by {1:P0}",
                SonarChannelHelper.GetDisplayName(channel),
                adjustment);

            bool succeeded =
                SteelSeriesSonarPlugin.Sonar?
                    .AdjustVolume(
                        channel,
                        adjustment)
                == true;

            if (succeeded)
            {
                SteelSeriesSonarPlugin.RefreshVariables();
            }
            else
            {
                MacroDeckLogger.Warning(
                    SteelSeriesSonarPlugin.Instance!,
                    "Failed to adjust {0} volume",
                    SonarChannelHelper.GetDisplayName(channel));
            }
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                SteelSeriesSonarPlugin.Instance!,
                "Adjust Sonar Volume failed: {0}",
                ex);
        }
    }

    public override ActionConfigControl GetActionConfigControl(
        ActionConfigurator actionConfigurator)
    {
        _ = actionConfigurator;

        return new AdjustSonarVolumeConfigControl(
            this);
    }
}
