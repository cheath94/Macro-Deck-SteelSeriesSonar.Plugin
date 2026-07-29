using SteelSeriesSonar.Plugin.Models;
using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.Variables;

namespace SteelSeriesSonar.Plugin.Variables;

public partial class SonarVariableManager
{
    private static readonly SonarChannel[] RedirectionChannels =
    [
        SonarChannel.Game,
        SonarChannel.ChatRender,
        SonarChannel.ChatCapture,
        SonarChannel.Media,
        SonarChannel.Aux
    ];

    public void UpdateRedirectionVariables(
        IReadOnlyCollection<StreamRedirection> redirections)
    {
        if (redirections.Count == 0)
        {
            return;
        }

        foreach (StreamerOutput output
                 in Enum.GetValues<StreamerOutput>())
        {
            string outputApiName =
                output switch
                {
                    StreamerOutput.Streaming =>
                        "streaming",

                    StreamerOutput.Monitoring =>
                        "monitoring",

                    _ =>
                        throw new ArgumentOutOfRangeException(
                            nameof(output),
                            output,
                            "Unsupported Streamer output.")
                };

            StreamRedirection? redirection =
                redirections.FirstOrDefault(
                    item =>
                        string.Equals(
                            item.Id,
                            outputApiName,
                            StringComparison.OrdinalIgnoreCase));

            if (redirection is null)
            {
                continue;
            }

            foreach (SonarChannel channel
                     in RedirectionChannels)
            {
                string role =
                    SonarChannelHelper.GetApiName(
                        channel);

                StreamRedirectionStatus? status =
                    redirection.Statuses.FirstOrDefault(
                        item =>
                            string.Equals(
                                item.Role,
                                role,
                                StringComparison.OrdinalIgnoreCase));

                if (status is null)
                {
                    continue;
                }

                string prefix =
                    GetStreamerVariablePrefix(
                        output,
                        channel);

                SetVariableIfChanged(
                    $"{prefix}_redirected",
                    status.IsEnabled,
                    VariableType.Bool);
            }
        }
    }
}
