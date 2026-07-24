using System.Globalization;
using Newtonsoft.Json.Linq;
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

        if (string.IsNullOrWhiteSpace(configuration))
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
