namespace SteelSeriesSonar.Plugin.Sonar;

public static class SonarChannelHelper
{
    public static string GetApiName(
        SonarChannel channel)
    {
        return channel switch
        {
            SonarChannel.Master =>
                "master",

            SonarChannel.Game =>
                "game",

            SonarChannel.ChatRender =>
                "chatRender",

            SonarChannel.ChatCapture =>
                "chatCapture",

            SonarChannel.Media =>
                "media",

            SonarChannel.Aux =>
                "aux",

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(channel),
                    channel,
                    "Unsupported Sonar channel.")
        };
    }

    public static string GetVariablePrefix(
        SonarChannel channel)
    {
        return channel switch
        {
            SonarChannel.Master =>
                "sonar_master",

            SonarChannel.Game =>
                "sonar_game",

            SonarChannel.ChatRender =>
                "sonar_chatrender",

            SonarChannel.ChatCapture =>
                "sonar_chatcapture",

            SonarChannel.Media =>
                "sonar_media",

            SonarChannel.Aux =>
                "sonar_aux",

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(channel),
                    channel,
                    "Unsupported Sonar channel.")
        };
    }

    public static string GetMuteVariableName(
        SonarChannel channel)
    {
        return $"{GetVariablePrefix(channel)}_muted";
    }

    public static string GetVolumeVariableName(
        SonarChannel channel)
    {
        return $"{GetVariablePrefix(channel)}_volume";
    }

    public static string GetDisplayName(
        SonarChannel channel)
    {
        return channel switch
        {
            SonarChannel.Master =>
                "Master",

            SonarChannel.Game =>
                "Game",

            SonarChannel.ChatRender =>
                "Chat",

            SonarChannel.ChatCapture =>
                "Mic",

            SonarChannel.Media =>
                "Media",

            SonarChannel.Aux =>
                "Aux",

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(channel),
                    channel,
                    "Unsupported Sonar channel.")
        };
    }
}