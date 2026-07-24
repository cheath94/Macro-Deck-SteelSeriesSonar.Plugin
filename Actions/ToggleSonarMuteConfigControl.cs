using Newtonsoft.Json.Linq;
using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Plugins;
using WinForms = System.Windows.Forms;

namespace SteelSeriesSonar.Plugin.Actions;

public class ToggleSonarMuteConfigControl : ActionConfigControl
{
    private readonly PluginAction action;
    private readonly WinForms.ComboBox channelBox;

    public ToggleSonarMuteConfigControl(
        PluginAction action)
    {
        this.action = action;

        channelBox =
            new WinForms.ComboBox
            {
                Left = 10,
                Top = 10,
                Width = 200,
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

        Controls.Add(channelBox);

        Width = 230;
        Height = 50;

        LoadExistingConfiguration();
    }

    private void LoadExistingConfiguration()
    {
        SelectChannel(
            SonarChannel.Game);

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

            SelectChannel(channel);
        }
        catch
        {
            SelectChannel(
                SonarChannel.Game);
        }
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
            $"Toggle mute: " +
            SonarChannelHelper.GetDisplayName(channel);

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