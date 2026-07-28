using SteelSeriesSonar.Plugin.Actions;
using SteelSeriesSonar.Plugin.Sonar;
using SteelSeriesSonar.Plugin.UI;
using SteelSeriesSonar.Plugin.Variables;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin;

public class SteelSeriesSonarPlugin : MacroDeckPlugin
{
    public static SteelSeriesSonarPlugin? Instance { get; private set; }

    public static SonarClient? Sonar { get; private set; }

    public static SonarVariableManager? Variables { get; private set; }

    public static SonarButtonStateManager? Buttons { get; private set; }

    public static SonarMonitor? Monitor { get; private set; }

    public SteelSeriesSonarPlugin()
    {
        Actions =
        [
            new SetSonarVolumeAction(),
            new AdjustSonarVolumeAction(),
            new ToggleSonarMuteAction(),
            new MuteSonarChannelAction(),
            new UnmuteSonarChannelAction(),
            new SetChatMixAction(),
            new AdjustChatMixAction()
        ];
    }

    public override void Enable()
    {
        CleanupServices();

        Instance = this;

        MacroDeckLogger.Information(
            this,
            "{0}",
            "SteelSeries Sonar plugin enabled");

        try
        {
            InitializeServices();

            if (Sonar?.Initialize() != true)
            {
                MacroDeckLogger.Error(
                    this,
                    "{0}",
                    "SteelSeries Sonar initialization failed");

                return;
            }

            MacroDeckLogger.Information(
                this,
                "{0}",
                "SteelSeries Sonar initialized successfully");

            RefreshVariables();

            StartMonitor();
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                this,
                "SteelSeries Sonar plugin initialization failed: {0}",
                ex);
        }
    }

    public static void RefreshVariables()
    {
        SonarClient? sonar = Sonar;
        SonarVariableManager? variables = Variables;

        if (sonar is null || variables is null)
        {
            return;
        }

        try
        {
            variables.UpdateVariables(
                sonar.GetAllChannelStates());
        }
        catch (Exception ex)
        {
            if (Instance is not null)
            {
                MacroDeckLogger.Error(
                    Instance,
                    "Unable to refresh Sonar variables: {0}",
                    ex);
            }
        }
    }

    private void InitializeServices()
    {
        Variables = new SonarVariableManager(this);

        Buttons = new SonarButtonStateManager(this);

        Variables.VariableChanged += Buttons.VariableChanged;

        Sonar = new SonarClient(this);
    }

    private void StartMonitor()
    {
        if (Sonar is null || Variables is null)
        {
            return;
        }

        Monitor = new SonarMonitor(
            this,
            Sonar,
            Variables);

        Monitor.Start();

        MacroDeckLogger.Information(
            this,
            "{0}",
            "Sonar variable monitor started");
    }

    private static void CleanupServices()
    {
        Monitor?.Dispose();
        Monitor = null;

        if (Variables is not null && Buttons is not null)
        {
            Variables.VariableChanged -= Buttons.VariableChanged;
        }

        Buttons = null;
        Variables = null;
        Sonar = null;
    }
}
