using static JumpenoWebassembly.Shared.Jumpeno.Enums;

namespace JumpenoWebassembly.Shared.Jumpeno
{
    /// <summary>
    /// Informacie, ktore sa menia pocas behu hry
    /// </summary>
    public class GameplayInfo
    {
        public float WinnerId { get; set; }
        public GameState State { get; set; }

        public int FramesToShrink { get; set; }
        public int FramesToScoreboard { get; set; }
        public int FramesToLobby { get; set; }

        public bool CountdownTimerRunning { get; set; }
        public bool ShrinkingAllowed { get; set; } = true;
        public bool GameoverTimerRunning { get; set; }
        public bool ScoreboardTimerRunning { get; set; }
    }
}
