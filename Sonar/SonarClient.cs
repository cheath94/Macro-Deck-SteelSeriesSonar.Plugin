using System.Globalization;
using System.Text.Json;
using SteelSeriesSonar.Plugin.Models;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.Sonar;

public class SonarClient
{
    private const int VolumeUpdateDelayMilliseconds = 250;

    private const double VolumeVerificationTolerance =
        0.011;

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    private readonly MacroDeckPlugin plugin;
    private readonly HttpClient httpClient;

    private string? sonarAddress;

    public SonarClient(
        MacroDeckPlugin plugin)
    {
        this.plugin = plugin;

        httpClient =
            new HttpClient(
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        (_, _, _, _) => true
                });
    }

    public bool Initialize()
    {
        try
        {
            string corePath =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.CommonApplicationData),
                    @"SteelSeries\GG\coreProps.json");

            if (!File.Exists(corePath))
            {
                throw new FileNotFoundException(
                    "SteelSeries GG coreProps.json not found.",
                    corePath);
            }

            string coreJson =
                File.ReadAllText(corePath);

            using JsonDocument core =
                JsonDocument.Parse(coreJson);

            string? ggAddress =
                core.RootElement
                    .GetProperty("ggEncryptedAddress")
                    .GetString();

            if (string.IsNullOrWhiteSpace(ggAddress))
            {
                throw new InvalidOperationException(
                    "SteelSeries GG did not provide an encrypted address.");
            }

            string response =
                httpClient
                    .GetStringAsync(
                        $"https://{ggAddress}/subApps")
                    .GetAwaiter()
                    .GetResult();

            using JsonDocument json =
                JsonDocument.Parse(response);

            sonarAddress =
                json.RootElement
                    .GetProperty("subApps")
                    .GetProperty("sonar")
                    .GetProperty("metadata")
                    .GetProperty("webServerAddress")
                    .GetString();

            MacroDeckLogger.Information(
                plugin,
                "Sonar address: {0}",
                sonarAddress ?? "null");

            return !string.IsNullOrWhiteSpace(
                sonarAddress);
        }
        catch (Exception ex)
        {
            sonarAddress = null;

            MacroDeckLogger.Error(
                plugin,
                "Sonar initialization failed: {0}",
                ex);

            return false;
        }
    }

    public VolumeSettings? GetVolumeSettings()
    {
        if (string.IsNullOrWhiteSpace(sonarAddress))
            return null;

        try
        {
            string url =
                $"{sonarAddress}/volumeSettings/classic/";

            string json =
                httpClient
                    .GetStringAsync(url)
                    .GetAwaiter()
                    .GetResult();

            MacroDeckLogger.Debug(
                plugin,
                "Volume JSON: {0}",
                json);

            return JsonSerializer.Deserialize<VolumeSettings>(
                json,
                JsonOptions);
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                plugin,
                "Unable to read volume settings: {0}",
                ex);

            return null;
        }
    }

    public StreamerVolumeSettings?
        GetStreamerVolumeSettings()
    {
        if (string.IsNullOrWhiteSpace(sonarAddress))
            return null;

        try
        {
            string url =
                $"{sonarAddress}/volumeSettings/streamer";

            string json =
                httpClient
                    .GetStringAsync(url)
                    .GetAwaiter()
                    .GetResult();

            MacroDeckLogger.Debug(
                plugin,
                "Streamer volume JSON: {0}",
                json);

            return JsonSerializer
                .Deserialize<StreamerVolumeSettings>(
                    json,
                    JsonOptions);
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                plugin,
                "Unable to read Streamer volume settings: {0}",
                ex);

            return null;
        }
    }

    public List<StreamRedirection>?
        GetStreamRedirections()
    {
        if (string.IsNullOrWhiteSpace(sonarAddress))
            return null;

        try
        {
            string url =
                $"{sonarAddress}/streamRedirections";

            string json =
                httpClient
                    .GetStringAsync(url)
                    .GetAwaiter()
                    .GetResult();

            MacroDeckLogger.Debug(
                plugin,
                "Stream redirection JSON: {0}",
                json);

            return JsonSerializer
                .Deserialize<List<StreamRedirection>>(
                    json,
                    JsonOptions);
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                plugin,
                "Unable to read stream redirections: {0}",
                ex);

            return null;
        }
    }

    public ChatMix? GetChatMix()
    {
        if (string.IsNullOrWhiteSpace(sonarAddress))
            return null;

        try
        {
            string url =
                $"{sonarAddress}/v1/chatMix";

            string json =
                httpClient
                    .GetStringAsync(url)
                    .GetAwaiter()
                    .GetResult();

            MacroDeckLogger.Debug(
                plugin,
                "ChatMix JSON: {0}",
                json);

            return JsonSerializer.Deserialize<ChatMix>(
                json,
                JsonOptions);
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                plugin,
                "Unable to read ChatMix: {0}",
                ex);

            return null;
        }
    }

    public bool SetChatMix(
        double balance)
    {
        if (string.IsNullOrWhiteSpace(sonarAddress))
            return false;

        try
        {
            balance =
                Math.Clamp(
                    balance,
                    -1.0,
                    1.0);

            balance =
                Math.Round(
                    balance,
                    2,
                    MidpointRounding.AwayFromZero);

            if (Math.Abs(balance) < 0.005)
            {
                balance = 0.0;
            }

            string value =
                balance.ToString(
                    "0.00",
                    CultureInfo.InvariantCulture);

            string url =
                $"{sonarAddress}/v1/chatMix?balance={value}";

            MacroDeckLogger.Information(
                plugin,
                "Setting ChatMix balance to {0}",
                balance);

            if (!SendPutRequest(url))
                return false;

            Thread.Sleep(
                VolumeUpdateDelayMilliseconds);

            ChatMix? verifiedChatMix =
                GetChatMix();

            if (verifiedChatMix is null)
            {
                MacroDeckLogger.Error(
                    plugin,
                    "{0}",
                    "Unable to verify ChatMix balance");

                return false;
            }

            MacroDeckLogger.Debug(
                plugin,
                "ChatMix balance after change: {0}",
                verifiedChatMix.Balance);

            return true;
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                plugin,
                "Unable to set ChatMix balance: {0}",
                ex);

            return false;
        }
    }

    public bool AdjustChatMix(
        double amount)
    {
        if (string.IsNullOrWhiteSpace(sonarAddress))
            return false;

        ChatMix? currentChatMix =
            GetChatMix();

        if (currentChatMix is null)
            return false;

        double newBalance =
            Math.Clamp(
                currentChatMix.Balance + amount,
                -1.0,
                1.0);

        MacroDeckLogger.Information(
            plugin,
            "Adjusting ChatMix from {0:P0} to {1:P0}",
            currentChatMix.Balance,
            newBalance);

        return SetChatMix(
            newBalance);
    }

    public double GetVolume(
        SonarChannel channel)
    {
        SonarChannelState? state =
            GetChannelState(channel);

        return state?.Volume ?? 0;
    }

    public bool GetMute(
        SonarChannel channel)
    {
        SonarChannelState? state =
            GetChannelState(channel);

        return state?.Muted ?? false;
    }

    public SonarChannelState? GetChannelState(
        SonarChannel channel)
    {
        VolumeSettings? settings =
            GetVolumeSettings();

        if (settings is null)
            return null;

        return CreateChannelState(
            settings,
            channel);
    }

    public Dictionary<SonarChannel, SonarChannelState>
        GetAllChannelStates()
    {
        Dictionary<SonarChannel, SonarChannelState> states =
            new();

        VolumeSettings? settings =
            GetVolumeSettings();

        if (settings is null)
            return states;

        foreach (SonarChannel channel
                 in Enum.GetValues<SonarChannel>())
        {
            states[channel] =
                CreateChannelState(
                    settings,
                    channel);
        }

        return states;
    }

    public Dictionary<
        (StreamerOutput Output, SonarChannel Channel),
        ClassicVolume> GetAllStreamerChannelStates()
    {
        Dictionary<
            (StreamerOutput Output, SonarChannel Channel),
            ClassicVolume> states =
                new();

        StreamerVolumeSettings? settings =
            GetStreamerVolumeSettings();

        if (settings is null)
        {
            return states;
        }

        foreach (StreamerOutput output
                 in Enum.GetValues<StreamerOutput>())
        {
            foreach (SonarChannel channel
                     in Enum.GetValues<SonarChannel>())
            {
                states[(output, channel)] =
                    GetStreamerVolumeState(
                        settings,
                        channel,
                        output);
            }
        }

        return states;
    }

    public ClassicVolume? GetStreamerVolumeState(
        SonarChannel channel,
        StreamerOutput output)
    {
        StreamerVolumeSettings? settings =
            GetStreamerVolumeSettings();

        if (settings is null)
            return null;

        return GetStreamerVolumeState(
            settings,
            channel,
            output);
    }

    public double GetStreamerVolume(
        SonarChannel channel,
        StreamerOutput output)
    {
        ClassicVolume? state =
            GetStreamerVolumeState(
                channel,
                output);

        return state?.Volume ?? 0;
    }

    public bool GetStreamerMute(
        SonarChannel channel,
        StreamerOutput output)
    {
        ClassicVolume? state =
            GetStreamerVolumeState(
                channel,
                output);

        return state?.Muted ?? false;
    }

    public bool? GetStreamRedirectionEnabled(
        SonarChannel channel,
        StreamerOutput output)
    {
        if (!IsValidRedirectionChannel(channel))
            return null;

        List<StreamRedirection>? redirections =
            GetStreamRedirections();

        if (redirections is null)
            return null;

        string outputName =
            GetStreamerOutputApiName(output);

        string channelName =
            SonarChannelHelper.GetApiName(channel);

        StreamRedirection? redirection =
            redirections.FirstOrDefault(
                item =>
                    string.Equals(
                        item.Id,
                        outputName,
                        StringComparison.OrdinalIgnoreCase));

        if (redirection is null)
            return null;

        StreamRedirectionStatus? status =
            redirection.Statuses.FirstOrDefault(
                item =>
                    string.Equals(
                        item.Role,
                        channelName,
                        StringComparison.OrdinalIgnoreCase));

        return status?.IsEnabled;
    }

    public bool SetVolume(
        SonarChannel channel,
        double volume)
    {
        return SetVolumeInternal(
            channel,
            output: null,
            volume);
    }

    public bool AdjustVolume(
        SonarChannel channel,
        double amount)
    {
        SonarChannelState? state =
            GetChannelState(channel);

        if (state is null)
            return false;

        double newVolume =
            Math.Clamp(
                state.Volume + amount,
                0.0,
                1.0);

        MacroDeckLogger.Information(
            plugin,
            "Adjusting {0} volume from {1:P0} to {2:P0}",
            channel,
            state.Volume,
            newVolume);

        return SetVolume(
            channel,
            newVolume);
    }

    public bool SetMute(
        SonarChannel channel,
        bool muted)
    {
        return SetMuteInternal(
            channel,
            output: null,
            muted);
    }

    public bool ToggleMute(
        SonarChannel channel)
    {
        SonarChannelState? state =
            GetChannelState(channel);

        if (state is null)
            return false;

        MacroDeckLogger.Information(
            plugin,
            "{0} mute state: {1}",
            channel,
            state.Muted);

        return SetMute(
            channel,
            !state.Muted);
    }

    public bool SetStreamerVolume(
        SonarChannel channel,
        StreamerOutput output,
        double volume)
    {
        return SetVolumeInternal(
            channel,
            output,
            volume);
    }

    public bool AdjustStreamerVolume(
        SonarChannel channel,
        StreamerOutput output,
        double amount)
    {
        ClassicVolume? state =
            GetStreamerVolumeState(
                channel,
                output);

        if (state is null)
            return false;

        double newVolume =
            Math.Clamp(
                state.Volume + amount,
                0.0,
                1.0);

        MacroDeckLogger.Information(
            plugin,
            "Adjusting {0} {1} volume from {2:P0} to {3:P0}",
            channel,
            output,
            state.Volume,
            newVolume);

        return SetStreamerVolume(
            channel,
            output,
            newVolume);
    }

    public bool SetStreamerMute(
        SonarChannel channel,
        StreamerOutput output,
        bool muted)
    {
        return SetMuteInternal(
            channel,
            output,
            muted);
    }

    public bool ToggleStreamerMute(
        SonarChannel channel,
        StreamerOutput output)
    {
        ClassicVolume? state =
            GetStreamerVolumeState(
                channel,
                output);

        if (state is null)
            return false;

        MacroDeckLogger.Information(
            plugin,
            "{0} {1} mute state: {2}",
            channel,
            output,
            state.Muted);

        return SetStreamerMute(
            channel,
            output,
            !state.Muted);
    }

    public bool SetStreamRedirectionEnabled(
        SonarChannel channel,
        StreamerOutput output,
        bool enabled)
    {
        if (string.IsNullOrWhiteSpace(sonarAddress))
            return false;

        if (!IsValidRedirectionChannel(channel))
        {
            MacroDeckLogger.Warning(
                plugin,
                "{0} is not a valid stream redirection channel.",
                channel);

            return false;
        }

        try
        {
            string apiChannel =
                SonarChannelHelper.GetApiName(channel);

            string apiOutput =
                GetStreamerOutputApiName(output);

            string value =
                enabled
                    ? "true"
                    : "false";

            string url =
                $"{sonarAddress}/streamRedirections/" +
                $"{apiOutput}/redirections/" +
                $"{apiChannel}/isEnabled/{value}";

            MacroDeckLogger.Information(
                plugin,
                "Setting {0} redirection for {1} to {2}",
                channel,
                output,
                enabled);

            if (!SendPutRequest(url))
                return false;

            Thread.Sleep(
                VolumeUpdateDelayMilliseconds);

            bool? verifiedEnabled =
                GetStreamRedirectionEnabled(
                    channel,
                    output);

            if (!verifiedEnabled.HasValue)
            {
                MacroDeckLogger.Warning(
                    plugin,
                    "Unable to verify {0} redirection for {1}.",
                    channel,
                    output);

                return false;
            }

            MacroDeckLogger.Debug(
                plugin,
                "{0} redirection for {1} after change: {2}",
                channel,
                output,
                verifiedEnabled.Value);

            return verifiedEnabled.Value == enabled;
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                plugin,
                "Unable to set stream redirection: {0}",
                ex);

            return false;
        }
    }

    public bool ToggleStreamRedirection(
        SonarChannel channel,
        StreamerOutput output)
    {
        bool? enabled =
            GetStreamRedirectionEnabled(
                channel,
                output);

        if (!enabled.HasValue)
        {
            MacroDeckLogger.Warning(
                plugin,
                "Unable to determine {0} redirection state for {1}.",
                channel,
                output);

            return false;
        }

        return SetStreamRedirectionEnabled(
            channel,
            output,
            !enabled.Value);
    }

    private bool SetVolumeInternal(
        SonarChannel channel,
        StreamerOutput? output,
        double volume)
    {
        if (string.IsNullOrWhiteSpace(sonarAddress))
            return false;

        try
        {
            volume =
                Math.Clamp(
                    volume,
                    0.0,
                    1.0);

            string value =
                volume.ToString(
                    "0.00",
                    CultureInfo.InvariantCulture);

            string apiChannel =
                SonarChannelHelper.GetApiName(channel);

            string url =
                output.HasValue
                    ? BuildStreamerVolumeUrl(
                        apiChannel,
                        output.Value,
                        value)
                    : BuildClassicVolumeUrl(
                        apiChannel,
                        value);

            if (output.HasValue)
            {
                MacroDeckLogger.Information(
                    plugin,
                    "Setting {0} {1} volume to {2:P0}",
                    channel,
                    output.Value,
                    volume);
            }
            else
            {
                MacroDeckLogger.Information(
                    plugin,
                    "Setting {0} volume to {1:P0}",
                    channel,
                    volume);
            }

            if (!SendPutRequest(url))
                return false;

            Thread.Sleep(
                VolumeUpdateDelayMilliseconds);

            double verifiedVolume =
                output.HasValue
                    ? GetStreamerVolume(
                        channel,
                        output.Value)
                    : GetVolume(channel);

            MacroDeckLogger.Debug(
                plugin,
                "{0} volume after change: {1:P0}",
                output.HasValue
                    ? $"{channel} {output.Value}"
                    : channel.ToString(),
                verifiedVolume);

            return Math.Abs(
                    verifiedVolume - volume)
                < VolumeVerificationTolerance;
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                plugin,
                "Unable to set volume: {0}",
                ex);

            return false;
        }
    }

    private bool SetMuteInternal(
        SonarChannel channel,
        StreamerOutput? output,
        bool muted)
    {
        if (string.IsNullOrWhiteSpace(sonarAddress))
            return false;

        try
        {
            string apiChannel =
                SonarChannelHelper.GetApiName(channel);

            string value =
                muted
                    ? "true"
                    : "false";

            string url =
                output.HasValue
                    ? BuildStreamerMuteUrl(
                        apiChannel,
                        output.Value,
                        value)
                    : BuildClassicMuteUrl(
                        apiChannel,
                        value);

            if (output.HasValue)
            {
                MacroDeckLogger.Information(
                    plugin,
                    "Setting {0} {1} mute to {2}",
                    channel,
                    output.Value,
                    muted);
            }
            else
            {
                MacroDeckLogger.Information(
                    plugin,
                    "Setting {0} mute to {1}",
                    channel,
                    muted);
            }

            if (!SendPutRequest(url))
                return false;

            Thread.Sleep(
                VolumeUpdateDelayMilliseconds);

            bool verifiedMuted =
                output.HasValue
                    ? GetStreamerMute(
                        channel,
                        output.Value)
                    : GetMute(channel);

            MacroDeckLogger.Debug(
                plugin,
                "{0} mute state after change: {1}",
                output.HasValue
                    ? $"{channel} {output.Value}"
                    : channel.ToString(),
                verifiedMuted);

            return verifiedMuted == muted;
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                plugin,
                "Unable to set mute: {0}",
                ex);

            return false;
        }
    }

    private bool SendPutRequest(
        string url)
    {
        try
        {
            MacroDeckLogger.Debug(
                plugin,
                "PUT {0}",
                url);

            using HttpResponseMessage response =
                httpClient
                    .PutAsync(
                        url,
                        null)
                    .GetAwaiter()
                    .GetResult();

            if (response.IsSuccessStatusCode)
                return true;

            MacroDeckLogger.Error(
                plugin,
                "PUT request returned status code {0}.",
                response.StatusCode);

            return false;
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(
                plugin,
                "PUT request failed: {0}",
                ex);

            return false;
        }
    }

    private string BuildClassicVolumeUrl(
        string apiChannel,
        string value)
    {
        return
            $"{sonarAddress}/volumeSettings/classic/" +
            $"{apiChannel}/Volume/{value}";
    }

    private string BuildClassicMuteUrl(
        string apiChannel,
        string value)
    {
        return
            $"{sonarAddress}/volumeSettings/classic/" +
            $"{apiChannel}/Mute/{value}";
    }

    private string BuildStreamerVolumeUrl(
        string apiChannel,
        StreamerOutput output,
        string value)
    {
        string apiOutput =
            GetStreamerOutputApiName(output);

        return
            $"{sonarAddress}/volumeSettings/streamer/" +
            $"{apiOutput}/{apiChannel}/volume/{value}";
    }

    private string BuildStreamerMuteUrl(
        string apiChannel,
        StreamerOutput output,
        string value)
    {
        string apiOutput =
            GetStreamerOutputApiName(output);

        return
            $"{sonarAddress}/volumeSettings/streamer/" +
            $"{apiOutput}/{apiChannel}/isMuted/{value}";
    }

    private static string GetStreamerOutputApiName(
        StreamerOutput output)
    {
        return output switch
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
    }

    private static bool IsValidRedirectionChannel(
        SonarChannel channel)
    {
        return channel switch
        {
            SonarChannel.Game => true,
            SonarChannel.ChatRender => true,
            SonarChannel.ChatCapture => true,
            SonarChannel.Media => true,
            SonarChannel.Aux => true,
            _ => false
        };
    }

    private static SonarChannelState CreateChannelState(
        VolumeSettings settings,
        SonarChannel channel)
    {
        ClassicVolume volume =
            GetClassicVolume(
                settings,
                channel);

        return new SonarChannelState
        {
            Volume = volume.Volume,
            Muted = volume.Muted
        };
    }

    private static ClassicVolume GetClassicVolume(
        VolumeSettings settings,
        SonarChannel channel)
    {
        return channel switch
        {
            SonarChannel.Master =>
                settings.Masters.Classic,

            SonarChannel.Game =>
                settings.Devices.Game.Classic,

            SonarChannel.ChatRender =>
                settings.Devices.ChatRender.Classic,

            SonarChannel.ChatCapture =>
                settings.Devices.ChatCapture.Classic,

            SonarChannel.Media =>
                settings.Devices.Media.Classic,

            SonarChannel.Aux =>
                settings.Devices.Aux.Classic,

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(channel),
                    channel,
                    "Unsupported Sonar channel.")
        };
    }

    private static ClassicVolume GetStreamerVolumeState(
        StreamerVolumeSettings settings,
        SonarChannel channel,
        StreamerOutput output)
    {
        StreamerVolumeOutputs outputs =
            channel switch
            {
                SonarChannel.Master =>
                    settings.Masters.Stream,

                SonarChannel.Game =>
                    settings.Devices.Game.Stream,

                SonarChannel.ChatRender =>
                    settings.Devices.ChatRender.Stream,

                SonarChannel.ChatCapture =>
                    settings.Devices.ChatCapture.Stream,

                SonarChannel.Media =>
                    settings.Devices.Media.Stream,

                SonarChannel.Aux =>
                    settings.Devices.Aux.Stream,

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(channel),
                        channel,
                        "Unsupported Sonar channel.")
            };

        return output switch
        {
            StreamerOutput.Streaming =>
                outputs.Streaming,

            StreamerOutput.Monitoring =>
                outputs.Monitoring,

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(output),
                    output,
                    "Unsupported Streamer output.")
        };
    }
}