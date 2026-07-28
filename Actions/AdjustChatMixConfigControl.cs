using System.Globalization;
using Newtonsoft.Json.Linq;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Plugins;
using WinForms = System.Windows.Forms;

namespace SteelSeriesSonar.Plugin.Actions;

public class AdjustChatMixConfigControl : ActionConfigControl
{
    private const double DefaultAdjustment = 0.05;

    private readonly PluginAction action;
    private readonly WinForms.ComboBox adjustmentBox;

    public AdjustChatMixConfigControl(
        PluginAction action)
    {
        this.action = action;

        adjustmentBox =
            new WinForms.ComboBox
            {
                Left = 10,
                Top = 10,
                Width = 220,
                DropDownStyle =
                    WinForms.ComboBoxStyle.DropDownList,
                DisplayMember =
                    nameof(AdjustmentOption.DisplayName),
                ValueMember =
                    nameof(AdjustmentOption.Value)
            };

        AddAdjustmentOptions();

        Controls.Add(adjustmentBox);

        Width = 250;
        Height = 60;

        LoadExistingConfiguration();
    }

    private void AddAdjustmentOptions()
    {
        double[] adjustments =
        {
            0.01,
            0.02,
            0.05,
            0.10,
            0.25,
            -0.01,
            -0.02,
            -0.05,
            -0.10,
            -0.25
        };

        foreach (double adjustment in adjustments)
        {
            adjustmentBox.Items.Add(
                new AdjustmentOption(
                    adjustment,
                    FormatAdjustment(adjustment)));
        }
    }

    private void LoadExistingConfiguration()
    {
        SelectAdjustment(
            DefaultAdjustment);

        if (string.IsNullOrWhiteSpace(
                action.Configuration))
        {
            return;
        }

        try
        {
            JObject config =
                JObject.Parse(
                    action.Configuration);

            double adjustment =
                ParseAdjustment(
                    config["adjustment"]?.ToString());

            SelectAdjustment(
                adjustment);
        }
        catch
        {
            SelectAdjustment(
                DefaultAdjustment);
        }
    }

    public override bool OnActionSave()
    {
        AdjustmentOption option =
            adjustmentBox.SelectedItem
                as AdjustmentOption
            ?? new AdjustmentOption(
                DefaultAdjustment,
                FormatAdjustment(DefaultAdjustment));

        JObject config =
            new()
            {
                ["adjustment"] =
                    option.Value.ToString(
                        "0.00",
                        CultureInfo.InvariantCulture)
            };

        action.Configuration =
            config.ToString();

        action.ConfigurationSummary =
            option.DisplayName;

        return true;
    }

    private void SelectAdjustment(
        double adjustment)
    {
        for (int index = 0;
             index < adjustmentBox.Items.Count;
             index++)
        {
            if (adjustmentBox.Items[index]
                is AdjustmentOption option &&
                Math.Abs(option.Value - adjustment) < 0.000001)
            {
                adjustmentBox.SelectedIndex =
                    index;

                return;
            }
        }

        adjustmentBox.SelectedIndex = 0;
    }

    private static double ParseAdjustment(
        string? value)
    {
        if (double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double adjustment))
        {
            return adjustment;
        }

        return DefaultAdjustment;
    }

    private static string FormatAdjustment(
        double adjustment)
    {
        int percent =
            (int)Math.Round(
                Math.Abs(adjustment) * 100,
                MidpointRounding.AwayFromZero);

        return adjustment >= 0
            ? $"{percent}% Toward Chat"
            : $"{percent}% Toward Game";
    }

    private sealed class AdjustmentOption
    {
        public AdjustmentOption(
            double value,
            string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }

        public double Value { get; }

        public string DisplayName { get; }
    }
}