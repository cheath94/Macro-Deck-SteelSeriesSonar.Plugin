using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.Actions;

public class AdjustChatMixAction : PluginAction
{
    private const string ActionName = "Adjust ChatMix";

    public override string Name =>
        ActionName;

    public override string Description =>
        "Moves the SteelSeries Sonar ChatMix balance toward Game or Chat.";

    public override bool CanConfigure =>
        true;

    public override void Trigger(
        string clientId,
        ActionButton actionButton)
    {
        _ = clientId;
        _ = actionButton;

        if (!SonarActionConfiguration.TryReadDouble(
                Configuration,
                ActionName,
                "adjustment",
                -1.0,
                1.0,
                out double adjustment))
        {
            return;
        }

        try
        {
            MacroDeckLogger.Debug(
                SteelSeriesSonarPlugin.Instance!,
                "Adjusting ChatMix by {0:P0}",
                adjustment);

            bool succeeded =
                SteelSeriesSonarPlugin.Sonar?
                    .AdjustChatMix(adjustment)
                == true;

            if (!succeeded)
            {
                MacroDeckLogger.Warning(
                    SteelSeriesSonarPlugin.Instance!,
                    "{0}",
                    "Failed to adjust ChatMix");
            }
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                SteelSeriesSonarPlugin.Instance!,
                "Adjust ChatMix failed: {0}",
                ex);
        }
    }

    public override ActionConfigControl GetActionConfigControl(
        ActionConfigurator actionConfigurator)
    {
        _ = actionConfigurator;

        return new AdjustChatMixConfigControl(
            this);
    }
}