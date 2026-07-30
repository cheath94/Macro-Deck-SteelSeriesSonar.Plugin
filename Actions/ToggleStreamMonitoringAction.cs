using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.Actions;

public class ToggleStreamMonitoringAction :
    PluginAction
{
    private const string ActionName =
        "Toggle Stream Output Monitoring";

    public override string Name =>
        ActionName;

    public override string Description =>
        "Enables or disables monitoring of the Sonar stream output.";

    public override bool CanConfigure =>
        false;

    public override void Trigger(
        string clientId,
        ActionButton actionButton)
    {
        _ = clientId;
        _ = actionButton;

        try
        {
            MacroDeckLogger.Debug(
                SteelSeriesSonarPlugin.Instance!,
                "{0}",
                "Toggling stream output monitoring");

            bool succeeded =
                SteelSeriesSonarPlugin.Sonar?
                    .ToggleStreamMonitoring()
                == true;

            if (succeeded)
            {
                SteelSeriesSonarPlugin.RefreshVariables();
            }
            else
            {
                MacroDeckLogger.Warning(
                    SteelSeriesSonarPlugin.Instance!,
                    "{0}",
                    "Failed to toggle stream output monitoring");
            }
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                SteelSeriesSonarPlugin.Instance!,
                "Toggle Stream Output Monitoring failed: {0}",
                ex);
        }
    }
}