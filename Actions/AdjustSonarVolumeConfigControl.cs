using System.Globalization;
using Newtonsoft.Json.Linq;
using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Plugins;
using WinForms = System.Windows.Forms;

namespace SteelSeriesSonar.Plugin.Actions;

public class AdjustSonarVolumeConfigControl : ActionConfigControl
{
    private const double DefaultAdjustment = 0.05;

    private readonly PluginAction action;
    private readonly WinForms.ComboBox channelBox;
    private readonly WinForms.ComboBox adjustmentBox;

    public AdjustSonarVolumeConfigControl(
        PluginAction action)
    {
        this.action = action;

        channelBox =
            new WinForms.ComboBox
            {
                Left = 10,
                Top = 10,
                Width = 220,
                DropDownStyle =
                    WinForms.ComboBoxStyle.DropDownList,
                DisplayMember =
                    nameof(ChannelOption.DisplayName),
                ValueMember =
                    nameof(ChannelOption.Channel)
            };

        foreach (SonarChannel channel
                 in Enum.GetValues<SonarChannel>())
        {
            channelBox.Items.Add(
                new ChannelOption(
                    channel,
                    SonarChannelHelper.GetDisplayName(channel)));
        }

        adjustmentBox =
            new WinForms.ComboBox
            {
                Left = 10,
                Top = 50,
                Width = 220,
                DropDownStyle =
                    WinForms.ComboBoxStyle.DropDownList,
                DisplayMember =
                    nameof(AdjustmentOption.DisplayName),
                ValueMember =
                    nameof(AdjustmentOption.Value)
            };

        AddAdjustmentOptions();

        Controls.Add(channelBox);
        Controls.Add(adjustmentBox);

        Width = 250;
        Height = 100;

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
        SelectChannel(
            SonarChannel.Game);

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

            SonarChannel channel =
                ParseChannel(
                    config["channel"]?.ToString());

            double adjustment =
                ParseAdjustment(
                    config["adjustment"]?.ToString());

            SelectChannel(channel);
            SelectAdjustment(adjustment);
        }
        catch
        {
            SelectChannel(
                SonarChannel.Game);

            SelectAdjustment(
                DefaultAdjustment);
        }
    }

    public override bool OnActionSave()
    {
        SonarChannel channel =
            channelBox.SelectedItem
                is ChannelOption channelOption
                    ? channelOption.Channel
                    : SonarChannel.Game;

        AdjustmentOption adjustmentOption =
            adjustmentBox.SelectedItem
                as AdjustmentOption
            ?? new AdjustmentOption(
                DefaultAdjustment,
                FormatAdjustment(DefaultAdjustment));

        JObject config =
            new()
            {
                ["channel"] =
                    channel.ToString(),

                ["adjustment"] =
                    adjustmentOption.Value.ToString(
                        "0.00",
                        CultureInfo.InvariantCulture)
            };

        action.Configuration =
            config.ToString();

        action.ConfigurationSummary =
            $"{SonarChannelHelper.GetDisplayName(channel)} " +
            $"{adjustmentOption.DisplayName}";

        return true;
    }

    private void SelectChannel(
        SonarChannel channel)
    {
        for (int index = 0;
             index < channelBox.Items.Count;
             index++)
        {
            if (channelBox.Items[index]
                is ChannelOption option &&
                option.Channel == channel)
            {
                channelBox.SelectedIndex =
                    index;

                return;
            }
        }

        channelBox.SelectedIndex = 0;
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
                Math.Abs(
                    option.Value - adjustment) < 0.000001)
            {
                adjustmentBox.SelectedIndex =
                    index;

                return;
            }
        }

        SelectAdjustment(
            DefaultAdjustment);
    }

    private static SonarChannel ParseChannel(
        string? value)
    {
        if (Enum.TryParse(
                value,
                ignoreCase: true,
                out SonarChannel channel))
        {
            return channel;
        }

        return SonarChannel.Game;
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
                adjustment * 100,
                MidpointRounding.AwayFromZero);

        return $"{percent:+0;-0}%";
    }

    private sealed class ChannelOption
    {
        public ChannelOption(
            SonarChannel channel,
            string displayName)
        {
            Channel = channel;
            DisplayName = displayName;
        }

        public SonarChannel Channel { get; }

        public string DisplayName { get; }
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