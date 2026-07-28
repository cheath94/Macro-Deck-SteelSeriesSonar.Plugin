using System.Globalization;
using Newtonsoft.Json.Linq;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Plugins;
using WinForms = System.Windows.Forms;

namespace SteelSeriesSonar.Plugin.Actions;

public class SetChatMixConfigControl : ActionConfigControl
{
    private const int DefaultAmountPercent = 50;

    private readonly PluginAction action;
    private readonly WinForms.ComboBox directionBox;
    private readonly WinForms.NumericUpDown amountBox;
    private readonly WinForms.Label amountLabel;

    public SetChatMixConfigControl(
        PluginAction action)
    {
        this.action = action;

        WinForms.Label directionLabel =
            new()
            {
                Text = "Position:",
                Left = 10,
                Top = 10,
                AutoSize = true
            };

        directionBox =
            new WinForms.ComboBox
            {
                Left = 10,
                Top = 30,
                Width = 220,
                DropDownStyle =
                    WinForms.ComboBoxStyle.DropDownList,
                DisplayMember =
                    nameof(DirectionOption.DisplayName),
                ValueMember =
                    nameof(DirectionOption.Direction)
            };

        directionBox.Items.Add(
            new DirectionOption(
                ChatMixDirection.Balanced,
                "Balanced"));

        directionBox.Items.Add(
            new DirectionOption(
                ChatMixDirection.Game,
                "Toward Game"));

        directionBox.Items.Add(
            new DirectionOption(
                ChatMixDirection.Chat,
                "Toward Chat"));

        directionBox.SelectedIndexChanged +=
            DirectionBox_SelectedIndexChanged;

        amountLabel =
            new WinForms.Label
            {
                Text = "Amount (%):",
                Left = 10,
                Top = 70,
                AutoSize = true
            };

        amountBox =
            new WinForms.NumericUpDown
            {
                Left = 10,
                Top = 90,
                Width = 100,
                Minimum = 0,
                Maximum = 100,
                DecimalPlaces = 0,
                Increment = 1,
                Value = DefaultAmountPercent
            };

        WinForms.Label helpLabel =
            new()
            {
                Text =
                    "0% = balanced. 100% = Fully Game/Chat.",
                Left = 10,
                Top = 120,
                Width = 300,
                Height = 155,
                AutoSize = false
            };

        Controls.Add(directionLabel);
        Controls.Add(directionBox);
        Controls.Add(amountLabel);
        Controls.Add(amountBox);
        Controls.Add(helpLabel);

        Width = 260;
        Height = 175;

        LoadExistingConfiguration();
    }

    private void DirectionBox_SelectedIndexChanged(
        object? sender,
        EventArgs e)
    {
        _ = sender;
        _ = e;

        ChatMixDirection direction =
            GetSelectedDirection();

        bool amountEnabled =
            direction != ChatMixDirection.Balanced;

        amountLabel.Enabled =
            amountEnabled;

        amountBox.Enabled =
            amountEnabled;

        if (!amountEnabled)
        {
            amountBox.Value = 0;
        }
        else if (amountBox.Value == 0)
        {
            amountBox.Value =
                DefaultAmountPercent;
        }
    }

    private void LoadExistingConfiguration()
    {
        SelectDirection(
            ChatMixDirection.Balanced);

        amountBox.Value = 0;

        if (string.IsNullOrWhiteSpace(
                action.Configuration))
        {
            UpdateAmountControlState();
            return;
        }

        try
        {
            JObject config =
                JObject.Parse(
                    action.Configuration);

            double balance =
                ParseBalance(
                    config["balance"]?.ToString());

            balance =
                Math.Clamp(
                    balance,
                    -1.0,
                    1.0);

            ChatMixDirection direction =
                GetDirectionFromBalance(
                    balance);

            decimal amountPercent =
                (decimal)Math.Abs(balance * 100.0);

            SelectDirection(
                direction);

            amountBox.Value =
                Math.Clamp(
                    amountPercent,
                    amountBox.Minimum,
                    amountBox.Maximum);

            UpdateAmountControlState();
        }
        catch
        {
            SelectDirection(
                ChatMixDirection.Balanced);

            amountBox.Value = 0;

            UpdateAmountControlState();
        }
    }

    public override bool OnActionSave()
    {
        ChatMixDirection direction =
            GetSelectedDirection();

        double normalizedAmount =
            (double)amountBox.Value / 100.0;

        double balance =
            direction switch
            {
                ChatMixDirection.Game =>
                    -normalizedAmount,

                ChatMixDirection.Chat =>
                    normalizedAmount,

                _ =>
                    0.0
            };

        balance =
            Math.Round(
                balance,
                2,
                MidpointRounding.AwayFromZero);

        JObject config =
            new()
            {
                ["balance"] =
                    balance.ToString(
                        "0.00",
                        CultureInfo.InvariantCulture)
            };

        action.Configuration =
            config.ToString();

        action.ConfigurationSummary =
            FormatSummary(
                direction,
                amountBox.Value);

        return true;
    }

    private void UpdateAmountControlState()
    {
        ChatMixDirection direction =
            GetSelectedDirection();

        bool amountEnabled =
            direction != ChatMixDirection.Balanced;

        amountLabel.Enabled =
            amountEnabled;

        amountBox.Enabled =
            amountEnabled;

        if (!amountEnabled)
        {
            amountBox.Value = 0;
        }
    }

    private ChatMixDirection GetSelectedDirection()
    {
        return directionBox.SelectedItem
            is DirectionOption option
                ? option.Direction
                : ChatMixDirection.Balanced;
    }

    private void SelectDirection(
        ChatMixDirection direction)
    {
        for (int index = 0;
             index < directionBox.Items.Count;
             index++)
        {
            if (directionBox.Items[index]
                is DirectionOption option &&
                option.Direction == direction)
            {
                directionBox.SelectedIndex =
                    index;

                return;
            }
        }

        directionBox.SelectedIndex = 0;
    }

    private static ChatMixDirection GetDirectionFromBalance(
        double balance)
    {
        if (balance < 0)
            return ChatMixDirection.Game;

        if (balance > 0)
            return ChatMixDirection.Chat;

        return ChatMixDirection.Balanced;
    }

    private static double ParseBalance(
        string? value)
    {
        if (double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double balance))
        {
            return balance;
        }

        return 0.0;
    }

    private static string FormatSummary(
        ChatMixDirection direction,
        decimal amountPercent)
    {
        return direction switch
        {
            ChatMixDirection.Game =>
                $"Toward Game: {amountPercent:0}%",

            ChatMixDirection.Chat =>
                $"Toward Chat: {amountPercent:0}%",

            _ =>
                "Balanced"
        };
    }

    private enum ChatMixDirection
    {
        Balanced,
        Game,
        Chat
    }

    private sealed class DirectionOption
    {
        public DirectionOption(
            ChatMixDirection direction,
            string displayName)
        {
            Direction = direction;
            DisplayName = displayName;
        }

        public ChatMixDirection Direction { get; }

        public string DisplayName { get; }
    }
}