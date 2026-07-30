using SteelSeriesSonar.Plugin.Models;
using SuchByte.MacroDeck.Variables;

namespace SteelSeriesSonar.Plugin.Variables;

public partial class SonarVariableManager
{
    public void UpdateChatMixVariables(
        ChatMix chatMix)
    {
        double balance =
            Math.Clamp(
                chatMix.Balance,
                -1.0,
                1.0);

        if (Math.Abs(balance) < 0.005)
        {
            balance = 0.0;
        }

        float roundedBalance =
            (float)Math.Round(
                balance,
                3);

        int percentage =
            (int)Math.Round(
                Math.Abs(balance) * 100,
                MidpointRounding.AwayFromZero);

        string side =
            balance switch
            {
                < 0 => "Game",
                > 0 => "Chat",
                _ => "Balanced"
            };

        string displayText =
            side switch
            {
                "Game" =>
                    $"{percentage}% Game",

                "Chat" =>
                    $"{percentage}% Chat",

                _ =>
                    "Balanced"
            };

        SetVariableIfChanged(
            "sonar_chatmix_balance",
            roundedBalance,
            VariableType.Float);

        SetVariableIfChanged(
            "sonar_chatmix_percent",
            percentage.ToString(),
            VariableType.String);

        SetVariableIfChanged(
            "sonar_chatmix_side",
            side,
            VariableType.String);

        SetVariableIfChanged(
            "sonar_chatmix_text",
            displayText,
            VariableType.String);
    }
}
