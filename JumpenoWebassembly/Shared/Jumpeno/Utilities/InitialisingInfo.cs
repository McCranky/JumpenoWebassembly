using static JumpenoWebassembly.Shared.Jumpeno.Enums;

namespace JumpenoWebassembly.Shared.Jumpeno.Utilities
{
    /**
     * Trieda obsahuje informácie na základe ktorých môže byť vytvorená hra
     */
    public class InitialisingInfo
    {
        public string ErrorMessage { get; set; } = "";
        public string GameCode { get; set; }
        public string GameName { get; set; }
        public int PlayersLimit { get; set; } = 2;
        public GameMode GameMode { get; set; } = GameMode.Player;

        public static string GameModeToString(GameMode mode)
        {
            return mode switch {
                GameMode.Player => "Player",
                GameMode.Guided => "Guided",
                _ => "",
            };
        }
    }
}
