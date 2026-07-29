using System.Globalization;
using Newtonsoft.Json.Linq;
using SteelSeriesSonar.Plugin.Models;
using SteelSeriesSonar.Plugin.Sonar;
using SuchByte.MacroDeck.Logging;

namespace SteelSeriesSonar.Plugin.Actions;

internal static class SonarActionConfiguration
{
    public static bool TryReadChannel(
        string? configuration,
        string actionName,
        out SonarChannel channel)
    {
        channel = default;

        if (!TryParseConfiguration(
                configuration,
                actionName,
                out JObject config))
        {
            return false;
        }

        return TryReadChannel(
            config,
            actionName,
            out channel);
    }

    public static bool TryReadDouble(
        string? configuration,
        string actionName,
        string propertyName,
        double minimum,
        double maximum,
        out double value)
    {
        value = default;

        if (!TryParseConfiguration(
                configuration,
                actionName,
                out JObject config))
        {
            return false;
        }

        return TryReadDouble(
            config,
            actionName,
            propertyName,
            minimum,
            maximum,
            out value);
    }

    public static bool TryReadChannelAndDouble(
        string? configuration,
        string actionName,
        string propertyName,
        double minimum,
        double maximum,
        out SonarChannel channel,
        out double value)
    {
        channel = default;
        value = default;

        if (!TryParseConfiguration(
                configuration,
                actionName,
                out JObject config))
        {
            return false;
        }

        if (!TryReadChannel(
                config,
                actionName,
                out channel))
        {
            return false;
        }

        return TryReadDouble(
            config,
            actionName,
            propertyName,
            minimum,
            maximum,
            out value);
    }

    public static bool TryReadStreamerChannel(
        string? configuration,
        string actionName,
        out SonarChannel channel,
        out StreamerOutput output)
    {
        channel = default;
        output = default;

        if (!TryParseConfiguration(
                configuration,
                actionName,
                out JObject config))
        {
            return false;
        }

        if (!TryReadChannel(
                config,
                actionName,
                out channel))
        {
            return false;
        }

        return TryReadStreamerOutput(
            config,
            actionName,
            out output);
    }

    public static bool TryReadStreamerChannelAndDouble(
        string? configuration,
        string actionName,
        string propertyName,
        double minimum,
        double maximum,
        out SonarChannel channel,
        out StreamerOutput output,
        out double value)
    {
        channel = default;
        output = default;
        value = default;

        if (!TryParseConfiguration(
                configuration,
                actionName,
                out JObject config))
        {
            return false;
        }

        if (!TryReadChannel(
                config,
                actionName,
                out channel))
        {
            return false;
        }

        if (!TryReadStreamerOutput(
                config,
                actionName,
                out output))
        {
            return false;
        }

        return TryReadDouble(
            config,
            actionName,
            propertyName,
            minimum,
            maximum,
            out value);
    }

    private static bool TryReadChannel(
        JObject config,
        string actionName,
        out SonarChannel channel)
    {
        channel = default;

        string? channelName =
            config["channel"]?
                .ToString();

        if (!Enum.TryParse(
                channelName,
                ignoreCase: true,
                out channel))
        {
            MacroDeckLogger.Warning(
                SteelSeriesSonarPlugin.Instance!,
                "{0}: Invalid Sonar channel: {1}",
                actionName,
                channelName ?? "<missing>");

            return false;
        }

        return true;
    }

    private static bool TryReadStreamerOutput(
        JObject config,
        string actionName,
        out StreamerOutput output)
    {
        output = default;

        string? outputName =
            config["output"]?
                .ToString();

        if (!Enum.TryParse(
                outputName,
                ignoreCase: true,
                out output))
        {
            MacroDeckLogger.Warning(
                SteelSeriesSonarPlugin.Instance!,
                "{0}: Invalid Streamer output: {1}",
                actionName,
                outputName ?? "<missing>");

            return false;
        }

        return true;
    }

    private static bool TryReadDouble(
        JObject config,
        string actionName,
        string propertyName,
        double minimum,
        double maximum,
        out double value)
    {
        value = default;

        string? rawValue =
            config[propertyName]?
                .ToString();

        if (!double.TryParse(
                rawValue,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value))
        {
            MacroDeckLogger.Warning(
                SteelSeriesSonarPlugin.Instance!,
                "{0}: Invalid {1} value: {2}",
                actionName,
                propertyName,
                rawValue ?? "<missing>");

            return false;
        }

        value =
            Math.Clamp(
                value,
                minimum,
                maximum);

        return true;
    }

    private static bool TryParseConfiguration(
        string? configuration,
        string actionName,
        out JObject config)
    {
        config = new JObject();

        if (string.IsNullOrWhiteSpace(
                configuration))
        {
            MacroDeckLogger.Warning(
                SteelSeriesSonarPlugin.Instance!,
                "{0}: No configuration found",
                actionName);

            return false;
        }

        try
        {
            config =
                JObject.Parse(
                    configuration);

            return true;
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Warning(
                SteelSeriesSonarPlugin.Instance!,
                "{0}: Invalid configuration: {1}",
                actionName,
                ex.Message);

            return false;
        }
    }
}