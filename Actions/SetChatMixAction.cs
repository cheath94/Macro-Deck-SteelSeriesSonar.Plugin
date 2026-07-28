using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.Actions;

public class SetChatMixAction : PluginAction
{
    private const string ActionName = "Set ChatMix";

    public override string Name =>
        ActionName;

    public override string Description =>
        "Sets the SteelSeries Sonar ChatMix balance.";

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
                "balance",
                -1.0,
                1.0,
                out double balance))
        {
            return;
        }

        try
        {
            MacroDeckLogger.Debug(
                SteelSeriesSonarPlugin.Instance!,
                "Setting ChatMix balance to {0:P0}",
                balance);

            bool succeeded =
                SteelSeriesSonarPlugin.Sonar?
                    .SetChatMix(balance)
                == true;

            if (!succeeded)
            {
                MacroDeckLogger.Warning(
                    SteelSeriesSonarPlugin.Instance!,
                    "{0}",
                    "Failed to set ChatMix balance");
            }
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                SteelSeriesSonarPlugin.Instance!,
                "Set ChatMix failed: {0}",
                ex);
        }
    }

    public override ActionConfigControl GetActionConfigControl(
        ActionConfigurator actionConfigurator)
    {
        _ = actionConfigurator;

        return new SetChatMixConfigControl(
            this);
    }
}