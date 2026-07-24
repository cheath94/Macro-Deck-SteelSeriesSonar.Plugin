using System.Globalization;
using Newtonsoft.Json.Linq;
using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Plugins;
using WinForms = System.Windows.Forms;

namespace SteelSeriesSonar.Plugin.Actions;

public class SetSonarVolumeConfigControl : ActionConfigControl
{
    private const decimal DefaultVolumePercent = 50;

    private readonly PluginAction action;
    private readonly WinForms.ComboBox channelBox;
    private readonly WinForms.NumericUpDown volumeBox;

    public SetSonarVolumeConfigControl(
        PluginAction action,
        ActionConfigurator actionConfigurator)
    {
        this.action = action;

        _ = actionConfigurator;

        WinForms.Label channelLabel =
            new()
            {
                Text = "Channel:",
                Left = 10,
                Top = 10,
                AutoSize = true
            };

        channelBox =
            new WinForms.ComboBox
            {
                Left = 10,
                Top = 30,
                Width = 150,
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

        WinForms.Label volumeLabel =
            new()
            {
                Text = "Volume (%):",
                Left = 10,
                Top = 70,
                AutoSize = true
            };

        volumeBox =
            new WinForms.NumericUpDown
            {
                Left = 10,
                Top = 90,
                Width = 100,
                Minimum = 0,
                Maximum = 100,
                DecimalPlaces = 0,
                Increment = 1,
                Value = DefaultVolumePercent
            };

        Controls.Add(channelLabel);
        Controls.Add(channelBox);
        Controls.Add(volumeLabel);
        Controls.Add(volumeBox);

        Width = 250;
        Height = 150;

        LoadExistingConfiguration();
    }

    private void LoadExistingConfiguration()
    {
        SelectChannel(
            SonarChannel.Game);

        volumeBox.Value =
            DefaultVolumePercent;

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

            decimal volumePercent =
                ParseVolumePercent(
                    config["volume"]?.ToString());

            SelectChannel(channel);

            volumeBox.Value =
                Math.Clamp(
                    volumePercent,
                    volumeBox.Minimum,
                    volumeBox.Maximum);
        }
        catch
        {
            SelectChannel(
                SonarChannel.Game);

            volumeBox.Value =
                DefaultVolumePercent;
        }
    }

    public override bool OnActionSave()
    {
        SonarChannel channel =
            channelBox.SelectedItem
                is ChannelOption option
                    ? option.Channel
                    : SonarChannel.Game;

        decimal normalizedVolume =
            volumeBox.Value / 100m;

        JObject config =
            new()
            {
                ["channel"] =
                    channel.ToString(),

                ["volume"] =
                    normalizedVolume.ToString(
                        "0.00",
                        CultureInfo.InvariantCulture)
            };

        action.Configuration =
            config.ToString();

        action.ConfigurationSummary =
            $"{SonarChannelHelper.GetDisplayName(channel)}: " +
            $"{volumeBox.Value:0}%";

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

    private static decimal ParseVolumePercent(
        string? value)
    {
        if (decimal.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out decimal normalizedVolume))
        {
            return normalizedVolume * 100m;
        }

        return DefaultVolumePercent;
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