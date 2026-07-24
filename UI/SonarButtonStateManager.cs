using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.UI;

public class SonarButtonStateManager
{
    private readonly MacroDeckPlugin plugin;
    private readonly object syncRoot = new();

    private readonly List<RegisteredButton> buttons =
        new();

    public SonarButtonStateManager(
        MacroDeckPlugin plugin)
    {
        this.plugin = plugin;
    }

    public void RegisterButton(
        ActionButton button,
        string variable)
    {
        ArgumentNullException.ThrowIfNull(button);

        if (string.IsNullOrWhiteSpace(variable))
        {
            throw new ArgumentException(
                "A variable name is required.",
                nameof(variable));
        }

        lock (syncRoot)
        {
            RegisteredButton? existing =
                buttons.FirstOrDefault(
                    item =>
                        ReferenceEquals(
                            item.Button,
                            button));

            if (existing is not null)
            {
                if (existing.Variable == variable)
                {
                    return;
                }

                existing.Variable =
                    variable;

                MacroDeckLogger.Debug(
                    plugin,
                    "Updated Sonar button registration to {0}",
                    variable);

                return;
            }

            buttons.Add(
                new RegisteredButton(
                    button,
                    variable));
        }

        MacroDeckLogger.Debug(
            plugin,
            "Registered Sonar button for {0}",
            variable);
    }

    public void VariableChanged(
        string name,
        object value)
    {
        if (value is not bool state)
        {
            return;
        }

        ActionButton[] matchingButtons;

        lock (syncRoot)
        {
            matchingButtons =
                buttons
                    .Where(
                        item =>
                            string.Equals(
                                item.Variable,
                                name,
                                StringComparison.Ordinal))
                    .Select(
                        item => item.Button)
                    .ToArray();
        }

        foreach (ActionButton button
                 in matchingButtons)
        {
            button.State =
                state;
        }

        if (matchingButtons.Length > 0)
        {
            MacroDeckLogger.Debug(
                plugin,
                "Updated {0} Sonar button(s) for {1} = {2}",
                matchingButtons.Length,
                name,
                state);
        }
    }

    private sealed class RegisteredButton
    {
        public RegisteredButton(
            ActionButton button,
            string variable)
        {
            Button = button;
            Variable = variable;
        }

        public ActionButton Button { get; }

        public string Variable { get; set; }
    }
}