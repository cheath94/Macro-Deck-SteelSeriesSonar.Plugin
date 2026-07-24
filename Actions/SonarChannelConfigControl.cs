using Newtonsoft.Json.Linq;
using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Plugins;
using WinForms = System.Windows.Forms;

namespace SteelSeriesSonar.Plugin.Actions;

public class SonarChannelConfigControl : ActionConfigControl
{
    private readonly PluginAction action;
    private readonly WinForms.ComboBox channelBox;

    public SonarChannelConfigControl(
        PluginAction action)
    {
        this.action = action;

        channelBox =
            new WinForms.ComboBox
            {
                Left = 10,
                Top = 10,
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

        channelBox.SelectedIndex = 0;

        Controls.Add(channelBox);

        Width = 250;
        Height = 50;

        LoadExistingConfiguration();
    }

    private void LoadExistingConfiguration()
    {
        if (string.IsNullOrWhiteSpace(
                action.Configuration))
        {
            SelectChannel(
                SonarChannel.Game);

            return;
        }

        try
        {
            JObject config =
                JObject.Parse(
                    action.Configuration);

            string? channelName =
                config["channel"]?
                    .ToString();

            if (!Enum.TryParse(
                    channelName,
                    ignoreCase: true,
                    out SonarChannel channel))
            {
                channel =
                    SonarChannel.Game;
            }

            SelectChannel(channel);
        }
        catch
        {
            SelectChannel(
                SonarChannel.Game);
        }
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

    public override bool OnActionSave()
    {
        SonarChannel channel =
            channelBox.SelectedItem
                is ChannelOption option
                    ? option.Channel
                    : SonarChannel.Game;

        JObject config =
            new()
            {
                ["channel"] =
                    channel.ToString()
            };

        action.Configuration =
            config.ToString();

        action.ConfigurationSummary =
            SonarChannelHelper.GetDisplayName(
                channel);

        return true;
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