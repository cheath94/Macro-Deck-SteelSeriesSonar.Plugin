using System.Globalization;
using Newtonsoft.Json.Linq;
using SteelSeriesSonar.Plugin.Models;
using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Plugins;
using WinForms = System.Windows.Forms;

namespace SteelSeriesSonar.Plugin.Actions;

public class StreamerVolumeConfigControl : ActionConfigControl
{
    private const decimal DefaultVolumePercent = 50;

    private readonly PluginAction action;
    private readonly string propertyName;
    private readonly string valueLabelText;

    private readonly WinForms.ComboBox outputBox;
    private readonly WinForms.ComboBox channelBox;
    private readonly WinForms.NumericUpDown valueBox;

    public StreamerVolumeConfigControl(
        PluginAction action,
        string propertyName,
        string valueLabelText,
        decimal minimum,
        decimal maximum,
        decimal defaultValue)
    {
        this.action = action;
        this.propertyName = propertyName;
        this.valueLabelText = valueLabelText;

        WinForms.Label outputLabel =
            new()
            {
                Text = "Output:",
                Left = 10,
                Top = 10,
                AutoSize = true
            };

        outputBox =
            new WinForms.ComboBox
            {
                Left = 10,
                Top = 30,
                Width = 180,
                DropDownStyle =
                    WinForms.ComboBoxStyle.DropDownList
            };

        outputBox.Items.Add(
            StreamerOutput.Streaming);

        outputBox.Items.Add(
            StreamerOutput.Monitoring);

        WinForms.Label channelLabel =
            new()
            {
                Text = "Channel:",
                Left = 10,
                Top = 70,
                AutoSize = true
            };

        channelBox =
            new WinForms.ComboBox
            {
                Left = 10,
                Top = 90,
                Width = 180,
                DropDownStyle =
                    WinForms.ComboBoxStyle.DropDownList,
                DisplayMember =
                    nameof(ChannelOption.DisplayName)
            };

        foreach (SonarChannel channel
                 in Enum.GetValues<SonarChannel>())
        {
            channelBox.Items.Add(
                new ChannelOption(
                    channel,
                    SonarChannelHelper.GetDisplayName(channel)));
        }

        WinForms.Label valueLabel =
            new()
            {
                Text = valueLabelText,
                Left = 10,
                Top = 130,
                AutoSize = true
            };

        valueBox =
            new WinForms.NumericUpDown
            {
                Left = 10,
                Top = 150,
                Width = 100,
                Minimum = minimum,
                Maximum = maximum,
                DecimalPlaces = 0,
                Increment = 1,
                Value = defaultValue
            };

        Controls.Add(outputLabel);
        Controls.Add(outputBox);
        Controls.Add(channelLabel);
        Controls.Add(channelBox);
        Controls.Add(valueLabel);
        Controls.Add(valueBox);

        Width = 250;
        Height = 210;

        LoadExistingConfiguration(
            defaultValue);
    }

    public override bool OnActionSave()
    {
        StreamerOutput output =
            outputBox.SelectedItem
                is StreamerOutput selectedOutput
                    ? selectedOutput
                    : StreamerOutput.Streaming;

        SonarChannel channel =
            channelBox.SelectedItem
                is ChannelOption channelOption
                    ? channelOption.Channel
                    : SonarChannel.Game;

        double normalizedValue =
            (double)valueBox.Value / 100.0;

        JObject config =
            new()
            {
                ["output"] =
                    output.ToString(),

                ["channel"] =
                    channel.ToString(),

                [propertyName] =
                    normalizedValue.ToString(
                        "0.00",
                        CultureInfo.InvariantCulture)
            };

        action.Configuration =
            config.ToString();

        action.ConfigurationSummary =
            $"{output} - " +
            $"{SonarChannelHelper.GetDisplayName(channel)} - " +
            $"{valueBox.Value:+0;-0;0}%";

        return true;
    }

    private void LoadExistingConfiguration(
        decimal defaultValue)
    {
        SelectOutput(
            StreamerOutput.Streaming);

        SelectChannel(
            SonarChannel.Game);

        valueBox.Value =
            defaultValue;

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

            if (Enum.TryParse(
                    config["output"]?.ToString(),
                    ignoreCase: true,
                    out StreamerOutput output))
            {
                SelectOutput(output);
            }

            if (Enum.TryParse(
                    config["channel"]?.ToString(),
                    ignoreCase: true,
                    out SonarChannel channel))
            {
                SelectChannel(channel);
            }

            string? rawValue =
                config[propertyName]?
                    .ToString();

            if (double.TryParse(
                    rawValue,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double normalizedValue))
            {
                decimal percent =
                    (decimal)(normalizedValue * 100.0);

                valueBox.Value =
                    Math.Clamp(
                        percent,
                        valueBox.Minimum,
                        valueBox.Maximum);
            }
        }
        catch
        {
            SelectOutput(
                StreamerOutput.Streaming);

            SelectChannel(
                SonarChannel.Game);

            valueBox.Value =
                defaultValue;
        }
    }

    private void SelectOutput(
        StreamerOutput output)
    {
        for (int index = 0;
             index < outputBox.Items.Count;
             index++)
        {
            if (outputBox.Items[index]
                is StreamerOutput option &&
                option == output)
            {
                outputBox.SelectedIndex =
                    index;

                return;
            }
        }

        outputBox.SelectedIndex = 0;
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
}