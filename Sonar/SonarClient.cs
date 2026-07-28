using System.Globalization;
using System.Text.Json;
using SteelSeriesSonar.Plugin.Models;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SteelSeriesSonar.Plugin.Sonar;

public class SonarClient
{
    private const int VolumeUpdateDelayMilliseconds = 250;

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

    public ChatMix? GetChatMix()
    {
        if (string.IsNullOrWhiteSpace(sonarAddress))
            return null;

        try
        {
            string url =
                $"{sonarAddress}/chatMix";

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
                $"{sonarAddress}/chatMix?balance={value}";

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

        if (settings == null)
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

        if (settings == null)
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

    public bool SetVolume(
        SonarChannel channel,
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
                $"{sonarAddress}/volumeSettings/classic/" +
                $"{apiChannel}/Volume/{value}";

            MacroDeckLogger.Information(
                plugin,
                "Setting {0} volume to {1:P0}",
                channel,
                volume);

            if (!SendPutRequest(url))
                return false;

            Thread.Sleep(
                VolumeUpdateDelayMilliseconds);

            double verifiedVolume =
                GetVolume(channel);

            MacroDeckLogger.Debug(
                plugin,
                "Volume after change: {0}",
                verifiedVolume);

            return true;
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

    public bool AdjustVolume(
        SonarChannel channel,
        double amount)
    {
        if (string.IsNullOrWhiteSpace(sonarAddress))
            return false;

        SonarChannelState? state =
            GetChannelState(channel);

        if (state == null)
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
                $"{sonarAddress}/volumeSettings/classic/" +
                $"{apiChannel}/Mute/{value}";

            MacroDeckLogger.Information(
                plugin,
                "Setting {0} mute to {1}",
                channel,
                muted);

            return SendPutRequest(url);
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

    public bool ToggleMute(
        SonarChannel channel)
    {
        SonarChannelState? state =
            GetChannelState(channel);

        if (state == null)
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
}