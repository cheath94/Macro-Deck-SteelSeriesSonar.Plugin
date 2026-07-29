using SteelSeriesSonar.Plugin.Models;
using SteelSeriesSonar.Plugin.Variables;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.Sonar;

public sealed class SonarMonitor : IDisposable
{
    private static readonly TimeSpan PollInterval =
        TimeSpan.FromSeconds(1);

    private readonly SonarClient sonar;
    private readonly SonarVariableManager variables;
    private readonly MacroDeckPlugin plugin;

    private System.Threading.Timer? timer;
    private int pollInProgress;
    private bool disposed;

    public SonarMonitor(
        MacroDeckPlugin plugin,
        SonarClient sonar,
        SonarVariableManager variables)
    {
        this.plugin = plugin;
        this.sonar = sonar;
        this.variables = variables;
    }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(
            disposed,
            this);

        if (timer is not null)
        {
            return;
        }

        MacroDeckLogger.Information(
            plugin,
            "{0}",
            "Starting Sonar variable monitor");

        timer =
            new System.Threading.Timer(
                Monitor,
                null,
                PollInterval,
                PollInterval);
    }

    private void Monitor(
        object? state)
    {
        _ = state;

        if (disposed)
        {
            return;
        }

        if (Interlocked.Exchange(
                ref pollInProgress,
                1) != 0)
        {
            MacroDeckLogger.Debug(
                plugin,
                "{0}",
                "Skipping overlapping Sonar monitor poll");

            return;
        }

        try
        {
            if (disposed)
            {
                return;
            }

            UpdateClassicVariables();
            UpdateChatMixVariables();
            UpdateStreamerVariables();
            UpdateRedirectionVariables();

            MacroDeckLogger.Debug(
                plugin,
                "{0}",
                "Sonar monitor poll completed");
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                plugin,
                "Sonar monitor failed: {0}",
                ex);
        }
        finally
        {
            Volatile.Write(
                ref pollInProgress,
                0);
        }
    }

    private void UpdateClassicVariables()
    {
        Dictionary<SonarChannel, SonarChannelState> states =
            sonar.GetAllChannelStates();

        if (states.Count > 0)
        {
            variables.UpdateVariables(
                states);

            return;
        }

        MacroDeckLogger.Debug(
            plugin,
            "{0}",
            "Sonar monitor received no Classic channel states");
    }

    private void UpdateChatMixVariables()
    {
        ChatMix? chatMix =
            sonar.GetChatMix();

        if (chatMix is not null)
        {
            variables.UpdateChatMixVariables(
                chatMix);

            return;
        }

        MacroDeckLogger.Debug(
            plugin,
            "{0}",
            "Sonar monitor received no ChatMix state");
    }

    private void UpdateStreamerVariables()
    {
        Dictionary<
            (StreamerOutput Output, SonarChannel Channel),
            ClassicVolume> states =
                sonar.GetAllStreamerChannelStates();

        if (states.Count > 0)
        {
            variables.UpdateStreamerVariables(
                states);

            return;
        }

        MacroDeckLogger.Debug(
            plugin,
            "{0}",
            "Sonar monitor received no Streamer channel states");
    }

    private void UpdateRedirectionVariables()
    {
        List<StreamRedirection>? redirections =
            sonar.GetStreamRedirections();

        if (redirections is not null &&
            redirections.Count > 0)
        {
            variables.UpdateRedirectionVariables(
                redirections);

            return;
        }

        MacroDeckLogger.Debug(
            plugin,
            "{0}",
            "Sonar monitor received no redirection states");
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        MacroDeckLogger.Information(
            plugin,
            "{0}",
            "Stopping Sonar variable monitor");

        System.Threading.Timer? existingTimer =
            Interlocked.Exchange(
                ref timer,
                null);

        existingTimer?.Dispose();

        GC.SuppressFinalize(
            this);
    }
}
